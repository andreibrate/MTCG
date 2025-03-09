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
    internal class PackEP : IHttpEndpoint
    {
        private readonly PackHandler _packHandler;

        public PackEP(PackHandler packHandler)
        {
            _packHandler = packHandler;
        }

        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Method == HttpMethod.POST)
            {
                // Admin should be able to add packs
                // Verify if the request is authorized
                var authHeader = rq.Headers["Authorization"];
                if (authHeader == null || authHeader != "Bearer admin-mtcgToken")
                {
                    rs.SetClientError("Unauthorized: Only admin can create packs", 403);
                    return true;
                }

                if (string.IsNullOrWhiteSpace(rq.Content))
                {
                    rs.SetClientError("Pack data is missing or invalid", 400);
                    return true;
                }

                try
                {
                    // Deserialize the content into a Package object
                    var pack = JsonConvert.DeserializeObject<List<Card>>(rq.Content, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None,
                        Converters = { new Converter() }
                    });

                    // Validate the pack
                    if (pack == null || pack.Count != 5)
                    {
                        rs.SetClientError("A pack must contain exactly 5 cards", 400);
                        return true;
                    }

                    // Add the package to the repository via the PackHandler
                    bool success = _packHandler.CreatePack(pack);

                    if (success)
                    {
                        rs.SetSuccess("Pack created successfully", 201);
                    }
                    else
                    {
                        rs.SetServerError("Failed to save the pack");
                    }
                }
                catch (JsonException ex)
                {
                    // Catch deserialization errors
                    rs.SetClientError($"Failed to deserialize pack: {ex.Message}", 400);
                }
                catch (Exception ex)
                {
                    // Catch unexpected errors
                    rs.SetServerError($"An unexpected error occurred: {ex.Message}");
                }

                return true;    // Success
            }
            return false;   // Unhandled request
        }

    }
}
