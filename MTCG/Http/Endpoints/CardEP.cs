using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class CardEP : IHttpEndpoint
    {
        private readonly UserHandler _userHandler;
        private readonly CardHandler _cardHandler;
        public CardEP(UserHandler userHandler, CardHandler cardHandler)
        {
            _userHandler = userHandler;
            _cardHandler = cardHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.GET)
            {
                // validate token
                if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
                {
                    rs.SetClientError("Unauthorized - Missing or invalid token", 401);
                    return true;
                }

                string token = rq.Headers["Authorization"].Substring("Bearer ".Length);

                try
                {
                    // find user by token
                    var user = _userHandler.FindUserByToken(token);
                    if (user == null)
                    {
                        rs.SetClientError("Unauthorized - Invalid token", 401);
                        return true;
                    }

                    // retrieve user cards
                    var cards = _cardHandler.GetUserCards(user.Id);
                    Console.WriteLine($"User {user.Id} has {cards.Count()} cards");
                    foreach (var card in cards)
                    {
                        Console.WriteLine($"Card: {card.Id}, Name: {card.Name}, OwnerId: {card.OwnerId}");
                    }

                    // return cards as JSON
                    var response = new
                    {
                        message = "Cards retrieved successfully",
                        cards = cards.Select(card => new
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

                    rs.SetSuccess(JsonConvert.SerializeObject(response, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented
                    }), 201);
                }
                catch (Exception ex)
                {
                    rs.SetServerError($"An unexpected error occurred: {ex.Message}");
                }

                return true;
            }

            return false;
        }

    }
}
