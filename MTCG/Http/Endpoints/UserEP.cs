using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using MTCG.Models;
// using Newtonsoft.Json;

namespace MTCG.Http.Endpoints
{
    internal class UserEP : IHttpEndpoint
    {
        private readonly UserHandler _userHandler;

        public UserEP(UserHandler userHandler)
        {
            _userHandler = userHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.POST && rq.Path[1] == "users")
            {
                return HandlePostUserRequest(rq, rs);
            }
            if (rq.Method == HttpMethod.GET && rq.Path.Length == 3 && rq.Path[1] == "users")
            {
                return HandleGetUserRequest(rq, rs);
            }
            if (rq.Method == HttpMethod.PUT && rq.Path.Length == 3 && rq.Path[1] == "users")
            {
                return HandlePutUserRequest(rq, rs);
            }

            return false;
        }

        private bool HandlePostUserRequest(HttpRequest rq, HttpResponse rs)
        {
            if (string.IsNullOrEmpty(rq.Content))
            {
                rs.SetClientError("No content provided", 400);
                return true;
            }

            // Deserialize the JSON content into a User object
            var userData = JsonSerializer.Deserialize<User>(rq.Content); // getting error using Newtonsoft.Json
            if (userData == null || string.IsNullOrEmpty(userData.Username) || string.IsNullOrEmpty(userData.Password))
            {
                rs.SetClientError("Invalid data provided", 400);
                return true;
            }

            // Call the UserHandler to register the user
            bool registrationSuccess = _userHandler.RegisterUser(userData.Username, userData.Password);
            if (registrationSuccess)
            {
                rs.SetSuccess("User created", 201);
            }
            else
            {
                rs.SetClientError("User already exists", 409); // Conflict
            }
            return true;
        }

        private bool HandleGetUserRequest(HttpRequest rq, HttpResponse rs)
        {
            if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
            {
                rs.SetClientError("Unauthorized - Missing or invalid token", 401);
                return true;
            }

            string token = rq.Headers["Authorization"].Substring("Bearer ".Length);
            string requestedUsername = rq.Path[2];

            // Find User by Token
            var requestingUser = _userHandler.FindUserByToken(token);
            if (requestingUser == null)
            {
                rs.SetClientError("Unauthorized - Invalid token", 401);
                return true;
            }

            Console.WriteLine($"Requesting Username: {requestingUser.Username}");
            Console.WriteLine($"Requested Username:  {requestedUsername}");

            var user = _userHandler.GetUserByUsername(requestedUsername);
            Console.WriteLine($"Database Username:   {user?.Username}");

            if (user == null)
            {
                rs.SetClientError("User not found", 404);
                return true;
            }

            rs.SetJsonContentType();
            rs.SetSuccess(JsonSerializer.Serialize(new
            {
                user.Username,
                user.Bio,
                user.Image
            }), 200);
            return true;
        }

        private bool HandlePutUserRequest(HttpRequest rq, HttpResponse rs)
        {
            if (!rq.Headers.ContainsKey("Authorization") || !rq.Headers["Authorization"].StartsWith("Bearer "))
            {
                rs.SetClientError("Unauthorized - Missing or invalid token", 401);
                return true;
            }

            string token = rq.Headers["Authorization"].Substring("Bearer ".Length);
            string requestedUsername = rq.Path[2];

            var requestingUser = _userHandler.FindUserByToken(token);
            Console.WriteLine($"Requesting Username: {requestingUser?.Username}");
            Console.WriteLine($"Requested Username:  {requestedUsername}");
            if (requestingUser == null || requestingUser.Username != requestedUsername)
            {
                rs.SetClientError("Forbidden - Cannot update another user's profile", 403);
                return true;
            }

            if (string.IsNullOrWhiteSpace(rq.Content))
            {
                rs.SetClientError("Invalid request content", 400);
                return true;
            }

            var updatedData = JsonSerializer.Deserialize<Dictionary<string, string>>(rq.Content);
            if (updatedData == null || (!updatedData.ContainsKey("Name") && !updatedData.ContainsKey("Bio") && !updatedData.ContainsKey("Image")))
            {
                rs.SetClientError("Invalid data provided", 400);
                return true;
            }

            var user = _userHandler.GetUserByUsername(requestedUsername);
            if (user == null)
            {
                rs.SetClientError("User not found", 404);
                return true;
            }

            user.Username = updatedData.ContainsKey("Name") ? updatedData["Name"] : user.Username;
            user.Bio = updatedData.ContainsKey("Bio") ? updatedData["Bio"] : user.Bio;
            user.Image = updatedData.ContainsKey("Image") ? updatedData["Image"] : user.Image;

            _userHandler.UpdateUser(user);

            rs.SetSuccess("User profile updated successfully", 200);
            return true;
        }

    }
}
