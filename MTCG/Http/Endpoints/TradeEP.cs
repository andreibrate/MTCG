using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using MTCG.Enums;
using MTCG.Models;
using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class TradeEP : IHttpEndpoint
    {
        private readonly TradeHandler _tradeHandler;

        public TradeEP(TradeHandler tradeHandler)
        {
            _tradeHandler = tradeHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Path.Length > 0 && rq.Path[1] == "trades")
            {
                switch (rq.Method)
                {
                    case HttpMethod.GET:
                        return rq.Path.Length == 2 ? HandleGetTrades(rq, rs) : HandleGetSingleTrade(rq, rs);
                    case HttpMethod.POST:
                        return rq.Path.Length == 2 ? HandleCreateTrade(rq, rs) : HandleAcceptTrade(rq, rs);
                    case HttpMethod.DELETE:
                        return HandleDeleteTrade(rq, rs);
                }
            }
            return false;
        }

        private bool HandleGetTrades(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                Console.WriteLine("Fetching all trades...");
                var trades = _tradeHandler.GetTrades();

                // Transform the deals into a simple serializable format
                var response = trades.Select(trade => new
                {
                    Id = trade.Id,
                    TradedCardId = trade.TradedCardId,
                    WantedElement = trade.WantedElement.ToString(),
                    WantedTribe = trade.WantedTribe.ToString(),
                    WantedMinDamage = trade.WantedMinDamage,
                    CardDetails = trade.TradedCard != null ? new
                    {
                        trade.TradedCard.Id,
                        trade.TradedCard.Name,
                        trade.TradedCard.Damage,
                        trade.TradedCard.Element,
                        Type = trade.TradedCard is MonsterCard monsterCard
                            ? $"Monster ({monsterCard.Tribe})"
                            : "Spell"
                    } : null
                });

                rs.SetJsonContentType();
                rs.Content = JsonConvert.SerializeObject(response, Formatting.Indented);
                rs.SetSuccess("Trades retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                rs.SetServerError($"Failed to retrieve trades: {ex.Message}");
            }

            return true;
        }

        private bool HandleGetSingleTrade(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Path.Length < 3 || !Guid.TryParse(rq.Path[2], out var tradeId))
            {
                rs.SetClientError("Invalid trade ID format", 400);
                return true;
            }

            try
            {
                var deal = _tradeHandler.GetTradeById(tradeId);

                if (deal == null)
                {
                    rs.SetClientError("Trade not found", 404);
                    return true;
                }

                // Transform the deals into a simple serializable format
                var response = new
                {
                    Id = deal.Id,
                    TradedCardId = deal.TradedCardId,
                    WantedElement = deal.WantedElement.ToString(),
                    WantedTribe = deal.WantedTribe.ToString(),
                    WantedMinDamage = deal.WantedMinDamage,
                    CardDetails = deal.TradedCard != null ? new
                    {
                        deal.TradedCard.Id,
                        deal.TradedCard.Name,
                        deal.TradedCard.Damage,
                        deal.TradedCard.Element,
                        Type = deal.TradedCard is MonsterCard monsterCard
                            ? $"Monster ({monsterCard.Tribe})"
                            : "Spell"
                    } : null
                };

                rs.SetJsonContentType();
                rs.Content = JsonConvert.SerializeObject(response, Formatting.Indented);
                rs.SetSuccess("Trade retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                rs.SetServerError($"Failed to retrieve trade: {ex.Message}");
            }

            return true;
        }

        private bool HandleCreateTrade(HttpRequest rq, HttpResponse rs)
        {
            if (string.IsNullOrWhiteSpace(rq.Content))
            {
                rs.SetClientError("Invalid request", 400);
                return true;
            }

            try
            {
                Console.WriteLine($"Attempting to Create Trade...");

                // Parse the input as a dictionary to avoid errors
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(rq.Content);
                if (data == null || !data.ContainsKey("Id") || !data.ContainsKey("TradedCard") || !data.ContainsKey("WantedMinDamage"))
                {
                    rs.SetClientError("Invalid JSON format or missing required fields (Id, TradedCard, WantedMinDamage)", 400);
                    return true;
                }

                // extract values
                if (!Guid.TryParse(data["Id"].ToString(), out var id))
                {
                    id = Guid.NewGuid(); // Fallback to a new GUID
                }

                if (!Guid.TryParse(data["TradedCard"].ToString(), out var tradedCardId))
                {
                    rs.SetClientError("Invalid CardToTrade GUID format", 400);
                    return true;
                }

                if (!float.TryParse(data["WantedMinDamage"].ToString(), out var wantedMinDamage))
                {
                    rs.SetClientError("Invalid WantedMinDamage value", 400);
                    return true;
                }

                // validate element and tribe
                var wantedElement = data.ContainsKey("WantedElement") && Enum.TryParse(typeof(Element), data["WantedElement"].ToString(), out var element)
                    ? (Element)element
                    : Element.Normal;

                var wantedTribe = data.ContainsKey("WantedTribe") && Enum.TryParse(typeof(Tribe), data["WantedTribe"].ToString(), out var tribe)
                    ? (Tribe)tribe
                    : Tribe.Goblin;

                // load card
                var tradedCard = _tradeHandler.GetCardById(tradedCardId);
                if (tradedCard == null)
                {
                    rs.SetClientError("Card to trade not found", 404);
                    return true;
                }

                // create Trading object
                var deal = new Trading
                {
                    Id = id,
                    TradedCard = tradedCard,
                    TradedCardId = tradedCard.Id,
                    WantedElement = wantedElement,
                    WantedTribe = wantedTribe,
                    WantedMinDamage = wantedMinDamage
                };

                // create trade
                _tradeHandler.CreateTrade(deal);

                rs.SetSuccess("Trade created successfully", 201);
            }
            catch (Exception ex)
            {
                rs.SetServerError($"Failed to create trade: {ex.Message}");
            }

            return true;
        }

        private bool HandleAcceptTrade(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Path.Length < 3 || !Guid.TryParse(rq.Path[2], out var tradeId))
            {
                rs.SetClientError("Invalid trade ID format", 400);
                return true;
            }

            if (string.IsNullOrWhiteSpace(rq.Content) || !Guid.TryParse(rq.Content.Trim('"'), out var offeredCardId))
            {
                rs.SetClientError("Invalid card format", 400);
                return true;
            }

            try
            {
                // No tradecesting allowed here, this isn't Alabama
                var token = rq.Headers.ContainsKey("Authorization")
                    ? rq.Headers["Authorization"].Replace("Bearer ", "").Trim()
                    : null;

                if (string.IsNullOrWhiteSpace(token))
                {
                    rs.SetClientError("Missing or invalid Authorization header", 401);
                    return true;
                }

                var user = _tradeHandler.GetUserByToken(token);
                if (user == null)
                {
                    rs.SetClientError("Unauthorized", 401);
                    return true;
                }
                var deal = _tradeHandler.GetTradeById(tradeId);
                if (deal == null)
                {
                    rs.SetClientError("Trade not found", 404);
                    return true;
                }
                if (deal.TradedCard?.OwnerId == user.Id)
                {
                    rs.SetClientError("You cannot trade with yourself", 400);
                    return true;
                }

                // Try to accept the trade
                if (_tradeHandler.AcceptTrade(tradeId, offeredCardId))
                {
                    rs.SetSuccess("Trade accepted", 201);
                }
                else
                {
                    rs.SetClientError("Trade conditions not met", 400);
                }
            }
            catch (Exception ex)
            {
                rs.SetServerError($"Failed to accept trade: {ex.Message}");
            }

            return true;
        }

        private bool HandleDeleteTrade(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Path.Length < 3 || !Guid.TryParse(rq.Path[2], out var tradeId))
            {
                rs.SetClientError("Invalid trade ID format", 400);
                return true;
            }

            try
            {
                _tradeHandler.DeleteTrade(tradeId);
                rs.SetSuccess("Trade deleted successfully", 200);
            }
            catch (Exception ex)
            {
                rs.SetServerError($"Failed to delete trade: {ex.Message}");
            }

            return true;
        }

    }
}
