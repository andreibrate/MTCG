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

    }
}
