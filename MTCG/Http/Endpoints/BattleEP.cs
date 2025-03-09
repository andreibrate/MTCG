using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using MTCG.Models;
using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class BattleEP : IHttpEndpoint
    {
        private readonly BattleHandler _battleHandler;
        private readonly BattleQueue _battleQueue;

        public BattleEP(BattleHandler battleHandler, BattleQueue battleQueue)
        {
            _battleHandler = battleHandler;
            _battleQueue = battleQueue;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.POST && rq.Path[1] == "battles")
            {
                // Check if the Authorization-Header exists
                if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
                {
                    rs.SetClientError("Missing or invalid Authorization header", 400);
                    return true;
                }

                // Extract token from the Authorization header
                string playerToken = rq.Headers["Authorization"].Substring("Bearer ".Length);

                // Validate input tokens
                if (playerToken == null || string.IsNullOrEmpty(playerToken))
                {
                    rs.SetClientError("Player token is required", 400);
                    return true;
                }

                // Matchmaking: Check if another player is available in the queue
                if (_battleQueue.TryPairPlayers(playerToken, out var opponent))
                {
                    if (opponent != null)
                    {
                        try
                        {
                            // start battle
                            var (winner, loser) = _battleHandler.StartBattle(playerToken, opponent);
                            rs.SetJsonContentType();
                            rs.Content = JsonConvert.SerializeObject(new { Winner = winner, Loser = loser }, Formatting.Indented);
                            rs.SetSuccess("Battle completed", 200);
                        }
                        catch (InvalidOperationException ex)
                        {
                            rs.SetClientError(ex.Message, 400);
                        }
                    }
                    else
                    {
                        rs.SetClientError("Opponent token is invalid.", 500);
                    }
                }
                else
                {
                    // If no opponent is available, the player waits
                    rs.SetSuccess("Waiting for opponent...", 202);
                }

                return true;
            }
            return false;
        }
    }
}
