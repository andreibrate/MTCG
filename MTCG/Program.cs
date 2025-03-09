using MTCG.Http.Endpoints;
using MTCG.Http;
using MTCG.Data_Access;
using MTCG.Data_Access.Interfaces;
using MTCG.Business_Logic;
using Npgsql;
using System;
using System.Net;
using System.Threading;

namespace MTCG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            // initialize connection string for DB connection
            string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mtcgdb";
            // initialize http server
            HttpServer? server = null;

            try
            {
                Console.WriteLine("Starting MTCG...");

                // Initialize DB / connection
                //#if DEBUG
                Console.WriteLine("Cleaning up Database...");
                DbManager.CleanupTables(connectionString);
                //#endif
                Console.WriteLine("Initializing Database...");
                DbManager.InitializeDatabase(connectionString);

                // initialize BattleQueue
                BattleQueue battleQueue = new BattleQueue();

                // create repos
                IUserRepo userRepo = new UserRepo(connectionString);
                IPackRepo packRepo = new PackRepo(connectionString);
                ICardRepo cardRepo = new CardRepo(connectionString);
                IDeckRepo deckRepo = new DeckRepo(connectionString);
                ITradeRepo tradeRepo = new TradeRepo(connectionString);

                // initialize handlers
                UserHandler userHandler = new UserHandler(userRepo);
                PackHandler packHandler = new PackHandler(packRepo);
                TransactionHandler transactionHandler = new TransactionHandler(userHandler, packRepo);
                CardHandler cardHandler = new CardHandler(cardRepo);
                DeckHandler deckHandler = new DeckHandler(deckRepo, cardRepo);
                ScoreboardHandler scoreboardHandler = new ScoreboardHandler(userRepo);
                TradeHandler tradeHandler = new TradeHandler(tradeRepo, cardRepo, userRepo);
                BattleHandler battleHandler = new BattleHandler(userRepo, deckRepo, cardRepo);

                // setup http server
                Console.WriteLine("Setting up server...");
                server = new HttpServer();
                var serverThread = new Thread(() => server.Run());

                // register endpoints for http server
                server.RegisterEndpoint("users", new UserEP(userHandler));                       // Registers Endpoint for user registration
                server.RegisterEndpoint("sessions", new SessionsEP(userHandler));                // Registers Endpoint for user login
                server.RegisterEndpoint("packs", new PackEP(packHandler));                       // Registers Endpoint for card packs (creating packs)
                server.RegisterEndpoint("transactions", new TransactionEP(transactionHandler));  // Registers Endpoint for transactions regarding card packs
                server.RegisterEndpoint("cards", new CardEP(userHandler, cardHandler));          // Registers Endpoint for card listing
                server.RegisterEndpoint("deck", new DeckEP(userHandler, deckHandler));           // Registers Endpoint for managing the user deck
                server.RegisterEndpoint("stats", new StatsEP(userHandler));                      // Registers Endpoint for viewing user statistics
                server.RegisterEndpoint("scoreboard", new ScoreboardEP(scoreboardHandler));      // Registers Endpoint for viewing the scoreboard
                server.RegisterEndpoint("trades", new TradeEP(tradeHandler));                    // Registers Endpoint for trading cards
                server.RegisterEndpoint("battles", new BattleEP(battleHandler, battleQueue));    // Registers Endpoint for battles between players

                // start Server
                serverThread.Start();

                // open shut down option for server
                Console.WriteLine("Press 'q' to stop the server...");
                while (Console.ReadKey(true).Key != ConsoleKey.Q)
                {
                    Thread.Sleep(100); // to avoid unnecessary resource usage
                }
                server.Stop();
                serverThread.Join();
            }
            catch (NpgsqlException ex)
            {
                // handle PostgreSQL exceptions
                Console.WriteLine("A PostgreSQL exception occurred: " + ex.Message);
            }
            catch (Exception ex)
            {
                // generic exception handling for other unexpected errors
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                // cleanup
                Console.WriteLine("Stopping server...");
                server?.Stop();

                //#if DEBUG
                Console.WriteLine("Cleaning up Database...");
                DbManager.CleanupTables(connectionString);
                //#endif

                Console.WriteLine("Program ended");
            }
        }
    }
}


