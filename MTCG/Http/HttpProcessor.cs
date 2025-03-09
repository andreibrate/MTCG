using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Http
{
    internal class HttpProcessor
    {
        private TcpClient clientSocket;
        private HttpServer httpServer;
        public HttpProcessor(HttpServer httpServer, TcpClient clientSocket)
        {
            this.httpServer = httpServer;
            this.clientSocket = clientSocket;
        }

        public void Process()
        {

            // ----- 1. Read the HTTP-Request -----
            using var reader = new StreamReader(clientSocket.GetStream());
            var request = new HttpRequest(reader);
            request.Parse();

            // ----- 2. Do the processing -----
            using var writer = new StreamWriter(clientSocket.GetStream()) { AutoFlush = true };
            var response = new HttpResponse(writer);

            // Console.WriteLine(string.Join(",",httpServer.Endpoints.Keys));
            var endpoint = httpServer.Endpoints.ContainsKey(request.Path[1]) ? httpServer.Endpoints[request.Path[1]] : null;
            if (endpoint == null || !endpoint.HandleRequest(request, response))
            {
                //Thread.Sleep(10000);
                response.ResponseCode = 404;
                response.ResponseMessage = "Not Found";
                response.Content = "Not found!";
                response.Headers.Add("Content-Type", "text/html");
            }

            Console.WriteLine("----------------------------------------");
            // ----- 3. Write the HTTP-Response -----
            response.Send();
            writer.Flush();

            Console.WriteLine("========================================");
        }
    }
}
