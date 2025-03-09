using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class TransactionEP : IHttpEndpoint
    {
        private readonly TransactionHandler _transactionHandler;

        public TransactionEP(TransactionHandler transactionHandler)
        {
            _transactionHandler = transactionHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            // Check if the path is `/transactions/packs` and method is POST
            if (rq.Method == HttpMethod.POST && rq.Path.Length >= 3 &&
                rq.Path[1] == "transactions" && rq.Path[2] == "packs")
            {
                // Check for valid authorization token
                if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
                {
                    rs.SetClientError("Unauthorized - Missing or invalid token", 401);
                    return true;
                }

                string token = rq.Headers["Authorization"].Substring("Bearer ".Length);

                try
                {
                    // Call the Business Logic to handle purchasing a package
                    var (isSuccess, pack, errorMessage) = _transactionHandler.BuyPack(token);

                    if (isSuccess && pack != null)
                    {
                        var response = new
                        {
                            message = "Pack purchased successfully",
                            cards = pack.Select(card => new
                            {
                                card.Id,
                                card.Name,
                                card.Damage,
                                card.Element,
                                card.CardType,
                                card.OwnerId
                            }).ToList()
                        };

                        rs.SetSuccess(JsonConvert.SerializeObject(response, new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented
                        }), 201);
                    }
                    else
                    {
                        // Differentiate between failure reasons
                        if (errorMessage != null)
                        {
                            rs.SetClientError(errorMessage, 400);
                        }
                        else
                        {
                            rs.SetServerError("An unexpected error occurred");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Catch unexpected errors and return a server error
                    rs.SetServerError($"An unexpected error occurred: {ex.Message}");
                }

                return true;
            }

            return false; // Unhandled request
        }
    }
}
