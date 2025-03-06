using MTCG.Http.Endpoints;
using MTCG.Http;
// Data Access Layer
// Business Logic Layer
using System;
using System.Net;
using System.Threading;
// using Npgsql;

namespace MTCG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            // initialize connection string for DB connection
            string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mtcgdb";
            // Initialize the Http Server
            HttpServer? server = null;


        }
    }
}


