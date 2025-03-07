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
    internal class PackRepo : IPackRepo
    {
        private readonly string _connectionString;
        public PackRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Guid? GetAdminId()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM Users WHERE Token = 'admin-mtcgToken';";

            var result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            if (Guid.TryParse(result.ToString(), out var adminId))
            {
                return adminId;
            }

            return null;
        }

        public List<Card> GetCardsByIds(Guid[] cardIds)
        {
            var cards = new List<Card>();

            if (cardIds == null || cardIds.Length == 0)
            {
                Console.WriteLine("No Card IDs found");
                return cards;
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT Id, Name, Damage, Element, Tribe, CardType, OwnerId
                            FROM Cards
                            WHERE Id = ANY(@CardIds);
                        ";
                        command.Parameters.AddWithValue("@CardIds", cardIds);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var id = reader.GetGuid(0);
                                var name = reader.GetString(1);
                                var damage = reader.GetDouble(2);
                                var element = (Element)reader.GetInt32(3);
                                var cardType = (CardType)reader.GetInt32(5);
                                var ownerId = reader.GetGuid(6);

                                if (cardType == CardType.Monster)
                                {
                                    var tribe = (Tribe)reader.GetInt32(4);
                                    cards.Add(new MonsterCard(name, damage, element, tribe, ownerId) { Id = id });
                                }
                                else
                                {
                                    cards.Add(new SpellCard(name, damage, element, ownerId) { Id = id });
                                }
                                Console.WriteLine($"Card found: ID={id}, OwnerID={ownerId}, Name={name}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cards: {ex.Message}");
            }

            return cards;
        }

        public void DeletePackageById(Guid packageId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Packages WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", packageId);

            command.ExecuteNonQuery();
        }

        public List<Card>? GetAvailablePackage()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, CardIds FROM Packages LIMIT 1;";  // get first package available

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var packageId = reader.GetGuid(0);              // UUID column from Id
                var cardIds = reader.GetFieldValue<Guid[]>(1);  // UUID-Array column from CardIds

                var cards = GetCardsByIds(cardIds);

                Console.WriteLine($"Package {packageId} found with cards:");
                foreach (var card in cards)
                {
                    Console.WriteLine($"Card {card.Id}, OwnerId: {card.OwnerId}");
                }
                DeletePackageById(packageId);   // remove it from available list
                return cards;
            }

            return null;  // no packages available
        }

        public void AddCard(Card card, NpgsqlConnection connection)
        {
            using var command = connection.CreateCommand();

            if (card is MonsterCard monsterCard)
            {
                command.CommandText = @"
                    INSERT INTO Cards (Id, Name, Damage, Element, CardType, Tribe, OwnerId, IsLocked)
                    VALUES (@Id, @Name, @Damage, @Element, @CardType, @Tribe, @OwnerId, @IsLocked)
                    ON CONFLICT (Id) DO NOTHING;
                ";
                command.Parameters.AddWithValue("@Tribe", (int)monsterCard.Tribe);
            }
            else
            {
                command.CommandText = @"
                    INSERT INTO Cards (Id, Name, Damage, Element, CardType, OwnerId, IsLocked)
                    VALUES (@Id, @Name, @Damage, @Element, @CardType, @OwnerId, @IsLocked)
                    ON CONFLICT (Id) DO NOTHING;
                ";
            }
            command.Parameters.AddWithValue("@Id", card.Id);
            command.Parameters.AddWithValue("@Name", card.Name);
            command.Parameters.AddWithValue("@Damage", card.Damage);
            command.Parameters.AddWithValue("@Element", (int)card.Element);
            command.Parameters.AddWithValue("@CardType", (int)card.CardType);
            command.Parameters.AddWithValue("@OwnerId", card.OwnerId);
            command.Parameters.AddWithValue("@IsLocked", card.IsLocked);

            command.ExecuteNonQuery();
        }

        public bool AddPackage(Package package)
        {
            // all packs have exactly 5 cards (no more, no less)
            if (package.Cards.Count != 5)
            {
                throw new InvalidOperationException("All packs contain exactly 5 cards (no more, no less)");
            }

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var card in package.Cards)
                {
                    AddCard(card, connection);
                }

                var cardIds = package.Cards.Select(card => card.Id).ToArray();
                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Packages (CardIds) VALUES (@CardIds);";
                command.Parameters.AddWithValue("@CardIds", cardIds);

                command.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        // Check how many packages are available
        public int GetAvailablePackageCount()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Packages;";

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool TransferOwnership(List<Card> cards, Guid newOwnerId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var card in cards)
                {
                    // check if card exists
                    using var validationCommand = connection.CreateCommand();
                    validationCommand.CommandText = "SELECT COUNT(*) FROM Cards WHERE Id = @CardId;";
                    validationCommand.Parameters.AddWithValue("@CardId", card.Id);
                    var exists = (long?)validationCommand.ExecuteScalar() ?? 0;
                    if (exists == 0)
                    {
                        Console.WriteLine($"Card with ID {card.Id} was not found in DB");
                        continue; // skip the card
                    }

                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        UPDATE Cards
                        SET OwnerId = @NewOwnerId
                        WHERE Id = @CardId;
                    ";
                    command.Parameters.AddWithValue("@NewOwnerId", newOwnerId);
                    command.Parameters.AddWithValue("@CardId", card.Id);

                    Console.WriteLine($"SQL Command: {command.CommandText}");
                    Console.WriteLine($"Transferring card {card.Id} to owner {newOwnerId}");
                    var rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"Failed to update CardId {card.Id}. No rows affected.");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully updated CardId {card.Id} to OwnerId {newOwnerId}.");
                    }
                }

                transaction.Commit();

                // Update local Card objects
                foreach (var card in cards)
                {
                    card.OwnerId = newOwnerId;
                }
                Console.WriteLine($"All cards successfully transferred.");

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error transferring ownership: {ex.Message}");
                return false;
            }
        }



    }
}
