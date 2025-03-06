using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MTCG.Http.Endpoints;

namespace MTCG.Http
{
    internal class HttpServer
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly int port = 10001;
        private readonly IPAddress ip = IPAddress.Loopback;

        private TcpListener tcpListener;
        public Dictionary<string, IHttpEndpoint> Endpoints { get; private set; } = new Dictionary<string, IHttpEndpoint>();

        public HttpServer()
        {
            port = 10001;
            ip = IPAddress.Loopback;

            tcpListener = new TcpListener(ip, port);
        }

        public HttpServer(int port)
        {
            this.port = port;
            ip = IPAddress.Loopback;

            tcpListener = new TcpListener(ip, port);
        }
        public HttpServer(IPAddress ip, int port)
        {
            this.port = port;
            this.ip = ip;

            tcpListener = new TcpListener(ip, port);
        }

        public void Run()
        {
            tcpListener.Start();
            Console.WriteLine($"Server is running on {ip}:{port}");

            while (!cts.Token.IsCancellationRequested)
            {
                if (tcpListener.Pending())
                {
                    // ----- 0. Accept the TCP-Client and create the reader and writer -----
                    var clientSocket = tcpListener.AcceptTcpClient();
                    var httpProcessor = new HttpProcessor(this, clientSocket);
                    // ThreadPool for multiple threads
                    ThreadPool.QueueUserWorkItem(o => httpProcessor.Process());
                }
                Thread.Sleep(100); // reduce CPU load
            }
        }

        public void Stop()
        {
            cts.Cancel();
            tcpListener.Stop();
            Console.WriteLine("Server has been stopped.");
        }

        public void RegisterEndpoint(string path, IHttpEndpoint endpoint)
        {
            Endpoints.Add(path, endpoint);
        }
    }
}
