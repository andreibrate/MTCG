using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class DeckEP : IHttpEndpoint
    {
        private readonly UserHandler _userHandler;
        private readonly DeckHandler _deckHandler;
        public DeckEP(UserHandler userHandler, DeckHandler deckHandler)
        {
            _userHandler = userHandler;
            _deckHandler = deckHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.GET && rq.Path[1] == "deck")
            {
                return HandleGetRequest(rq, rs);
            }
            if (rq.Method == HttpMethod.PUT && rq.Path[1] == "deck")
            {
                return HandlePutRequest(rq, rs);
            }

            return false;
        }

        private bool HandleGetRequest(HttpRequest rq, HttpResponse rs)
        {
            if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
            {
                rs.SetClientError("Unauthorized - Missing or invalid token", 401);
                return true;
            }

            string token = rq.Headers["Authorization"].Substring("Bearer ".Length);

            var user = _userHandler.FindUserByToken(token);
            if (user == null)
            {
                rs.SetClientError("Unauthorized - Invalid token", 401);
                return true;
            }

            var deck = _deckHandler.GetDeckByUserId(user.Id);
            Console.WriteLine($"Deck Retrieved: {deck.Count()} cards for user {user.Id}");

            var response = new
            {
                message = "Deck retrieved successfully",
                cards = deck.Select(card => new
                {
                    card.Id,
                    card.Name,
                    card.Damage,
                    card.Element,
                    card.CardType,
                    card.OwnerId,
                    card.IsLocked
                }).ToList()
            };

            rs.SetSuccess(JsonConvert.SerializeObject(response, Formatting.Indented), 200);
            return true;
        }

        private bool HandlePutRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq == null || string.IsNullOrWhiteSpace(rq.Content))
            {
                rs.SetClientError("Invalid request", 400);
                return true;
            }

            if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
            {
                rs.SetClientError("Unauthorized - Missing or invalid token", 401);
                return true;
            }

            string token = rq.Headers["Authorization"].Substring("Bearer ".Length);

            var user = _userHandler.FindUserByToken(token);
            if (user == null)
            {
                rs.SetClientError("Unauthorized - Invalid token", 401);
                return true;
            }

            var cardIds = JsonConvert.DeserializeObject<List<Guid>>(rq.Content);
            if (cardIds == null || cardIds.Count() != 4)
            {
                rs.SetClientError("Bad request: Deck must contain exactly 4 valid card IDs", 400);
                return true;
            }

            try
            {
                if (_deckHandler.UpdateDeck(user.Id, cardIds))
                {
                    rs.SetSuccess("Deck updated successfully", 200);
                }
                else
                {
                    rs.SetServerError("Failed to update deck");
                }
            }
            catch (InvalidOperationException ex)
            {
                rs.SetClientError(ex.Message, 400);
            }

            return true;
        }

    }
}
