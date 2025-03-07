using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using MTCG.Models;
using MTCG.Enums;
using MTCG.Data_Access.Interfaces;

namespace MTCG.Data_Access
{
    public class UserRepo : IUserRepo
    {
        private readonly string _connectionString;
        public UserRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        // tokenize username
        private string GenerateToken(string username)
        {
            return $"{username}-mtcgToken";
        }

        public bool RegisterUser(string username, string password)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Id, Username, Password, Coins, Token, Elo, Wins, Losses)
                VALUES (@Id, @Username, @Password, 20, @Token, 100, 0, 0);
            ";
            var token = GenerateToken(username);
            command.Parameters.AddWithValue("@Id", Guid.NewGuid());
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Token", token);

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (PostgresException)
            {
                return false;   // catch duplicate username errors
            }
        }

        public User? LoginUser(string username, string password)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, Username, Password, Coins, Token, Elo, Wins, Losses
                        FROM Users
                        WHERE Username = @username AND Password = @password";
                    command.Parameters.AddWithValue("username", username);
                    command.Parameters.AddWithValue("password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var id = reader.GetGuid(0);
                            var coins = reader.GetInt32(3);
                            var token = reader.IsDBNull(4) ? null : reader.GetString(4);
                            var elo = reader.GetInt32(5);
                            var wins = reader.GetInt32(6);
                            var losses = reader.GetInt32(7);

                            if (string.IsNullOrEmpty(token))
                            {
                                token = GenerateToken(username);

                                // separate connection for update
                                using var updateConnection = new NpgsqlConnection(_connectionString);
                                updateConnection.Open();

                                using var updateCommand = updateConnection.CreateCommand();
                                updateCommand.CommandText = "UPDATE Users SET Token = @Token WHERE Id = @Id";
                                updateCommand.Parameters.AddWithValue("Token", token);
                                updateCommand.Parameters.AddWithValue("Id", id);
                                updateCommand.ExecuteNonQuery();
                            }

                            return new User(id, username, password, coins, token ?? string.Empty, elo, wins, losses);
                        }
                    }
                }
            }
            return null;
        }

        // find a user by their token
        public User? GetUserByToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, Username, Password, Coins, Token, Elo, Wins, Losses
                        FROM Users
                        WHERE Token = @token";
                    command.Parameters.AddWithValue("token", token);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var id = reader.GetGuid(0);
                            var username = reader.GetString(1);
                            var password = reader.GetString(2);
                            var coins = reader.GetInt32(3);
                            var elo = reader.GetInt32(5);
                            var wins = reader.GetInt32(6);
                            var losses = reader.GetInt32(7);

                            return new User(id, username, password, coins, token ?? string.Empty, elo, wins, losses);
                        }
                    }
                }
            }
            return null;
        }

        public List<User> GetUsers()    // all of them (for scoreboard)
        {
            var users = new List<User>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, Username, Password, Coins, Token, Elo, Wins, Losses
                        FROM Users
                        ORDER BY Elo DESC, Wins DESC, Losses ASC;";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User(
                                reader.GetGuid(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetInt32(3),
                                reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                reader.GetInt32(5),
                                reader.GetInt32(6),
                                reader.GetInt32(7)
                            ));
                        }
                    }
                }
            }

            return users;
        }

        public List<User> GetUsersStartingWith(string prefix)
        {
            var users = new List<User>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Username, Password, Coins, Token, Elo, Wins, Losses, Bio, Image
                FROM Users
                WHERE Username ILIKE @Username || '%';
            ";
            command.Parameters.AddWithValue("Username", prefix);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User(
                    id: reader.GetGuid(0),
                    username: reader.GetString(1),
                    password: reader.GetString(2),
                    coins: reader.GetInt32(3),
                    token: reader.GetString(4),
                    elo: reader.GetInt32(5),
                    wins: reader.GetInt32(6),
                    losses: reader.GetInt32(7),
                    bio: reader.IsDBNull(8) ? null : reader.GetString(8),
                    image: reader.IsDBNull(9) ? null : reader.GetString(9)
                ));
            }
            return users;
        }

        public void UpdateUser(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            // make sure user has a token
            if (string.IsNullOrWhiteSpace(user.Token))
            {
                user.Token = GenerateToken(user.Username);
            }

            command.CommandText = @"
                UPDATE Users
                SET 
                    Username = @Username,
                    Password = @Password,
                    Coins = @Coins,
                    Token = @Token,
                    Elo = @Elo,
                    Wins = @Wins,
                    Losses = @Losses,
                    Bio = @Bio,
                    Image = @Image
                WHERE Id = @Id;
            ";
            command.Parameters.AddWithValue("Id", user.Id);
            command.Parameters.AddWithValue("Username", user.Username ?? string.Empty);
            command.Parameters.AddWithValue("Password", user.Password ?? string.Empty);
            command.Parameters.AddWithValue("Coins", Math.Max(0, user.Coins));          // only positive coins
            command.Parameters.AddWithValue("Token", user.Token ?? string.Empty);       // default = empty string
            command.Parameters.AddWithValue("Elo", user.Stats?.Elo ?? 100);             // default = 100
            command.Parameters.AddWithValue("Wins", user.Stats?.Wins ?? 0);             // default = 0
            command.Parameters.AddWithValue("Losses", user.Stats?.Losses ?? 0);         // default = 0
            command.Parameters.AddWithValue("Bio", user.Bio ?? string.Empty);           // default = empty string
            command.Parameters.AddWithValue("Image", user.Image ?? string.Empty);       // default = empty string

            command.ExecuteNonQuery();
        }

        public IEnumerable<(string Username, Stats Stats)> GetScoreboardStats()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // sort all users by elo, wins, losses, username
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Username, Elo, Wins, Losses
                FROM Users
                ORDER BY Elo DESC, Wins DESC, Losses ASC, Username ASC;
            ";

            using var reader = command.ExecuteReader();
            var userScoreboardStats = new List<(string Username, Stats Stats)>();
            while (reader.Read())
            {
                var username = reader.GetString(0);
                var stats = new Stats(
                    Elo: reader.GetInt32(1),
                    Wins: reader.GetInt32(2),
                    Losses: reader.GetInt32(3)
                );

                userScoreboardStats.Add((username, stats));
            }

            return userScoreboardStats;
        }





    }
}
