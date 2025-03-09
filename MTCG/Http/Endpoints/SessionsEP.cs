using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using MTCG.Models;

namespace MTCG.Http.Endpoints
{
    internal class SessionsEP : IHttpEndpoint
    {
        private readonly UserHandler _userHandler;

        public SessionsEP(UserHandler userHandler)
        {
            _userHandler = userHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.POST && rq.Path[1] == "sessions")
            {
                if (string.IsNullOrEmpty(rq.Content))
                {
                    rs.SetClientError("No content provided", 400);
                    return true;
                }

                // Deserialize for login
                var userData = JsonSerializer.Deserialize<User>(rq.Content);
                if (userData == null || string.IsNullOrEmpty(userData.Username) || string.IsNullOrEmpty(userData.Password))
                {
                    rs.SetClientError("Invalid login data provided", 400);
                    return true;
                }

                // Call the UserHandler to log the user in
                string? token = _userHandler.LoginUser(userData.Username, userData.Password);
                if (token != null)
                {
                    rs.SetSuccess("Login successful", 200);
                    rs.Content = JsonSerializer.Serialize(new { Token = token });
                }
                else
                {
                    rs.SetClientError("Login failed", 401); // Unauthorized
                }
                return true;
            }

            return false;
        }
    }
}
