using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class StatsEP : IHttpEndpoint
    {
        private readonly UserHandler _userHandler;

        public StatsEP(UserHandler userHandler)
        {
            _userHandler = userHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.GET && rq.Path[1] == "stats")
            {
                if (!rq.Headers.ContainsKey("Authorization"))
                {
                    rs.SetClientError("Authorization header is missing", 401);
                    return true;
                }

                // Extract token from Authorization header
                string? token = rq.Headers["Authorization"].Split(' ')[1];
                if (string.IsNullOrEmpty(token))
                {
                    rs.SetClientError("Token is invalid", 401);
                    return true;
                }

                var stats = _userHandler.GetStatsByToken(token);

                if (stats == null)
                {
                    rs.SetClientError("User not found or invalid token", 404);
                    return true;
                }

                rs.Content = JsonConvert.SerializeObject(stats, Formatting.Indented);
                rs.SetSuccess("Stats retrieved successfully", 200);
                return true;
            }

            return false;
        }
    }
}
