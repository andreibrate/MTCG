using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Http
{
    public class HttpResponse
    {
        private StreamWriter writer;

        public string HttpVersion { get; set; } = "HTTP/1.0";
        public int ResponseCode { get; set; } = 200;
        public string ResponseMessage { get; set; } = "OK";
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string? Content { get; set; }

        public HttpResponse(StreamWriter writer)
        {
            this.writer = writer;
        }

        public void Send()
        {
            var writerAlsoToConsole = new StreamTracer(writer);

            writerAlsoToConsole.WriteLine($"{HttpVersion} {ResponseCode} {ResponseMessage}");

            if (Content != null)
            {
                Headers["Content-Length"] = Content.Length.ToString();
            }
            foreach (var header in Headers)
            {
                writerAlsoToConsole.WriteLine($"{header.Key}: {header.Value}");
            }
            writerAlsoToConsole.WriteLine();
            if (Content != null)
                writerAlsoToConsole.WriteLine(Content);
        }

        // Method to set success response
        public void SetSuccess(string message, int code)
        {
            ResponseMessage = message;
            ResponseCode = code;
        }

        // Method to set client error response
        public void SetClientError(string message, int code)
        {
            ResponseMessage = message;
            ResponseCode = code;
        }

        // Method to set server error response
        public void SetServerError(string message)
        {
            ResponseMessage = message;
            ResponseCode = 500; // Internal Server Error
        }

        public void SetJsonContentType()
        {
            Headers["Content-Type"] = "application/json";
        }
    }
}
