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
    public class TradeRepo : ITradeRepo
    {
        private readonly string _connectionString;

        public TradeRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddTrade(Trading trade)
        {
            if (trade.TradedCard == null)
            {
                throw new InvalidOperationException("TradedCard cannot be null when adding a trade");
            }

            // check if card is locked
            if (trade.TradedCard.IsLocked)
            {
                throw new InvalidOperationException("TradedCard is locked and can't be added to a trade");
            }

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // lock the card before adding it to the trade
                using (var lockCommand = connection.CreateCommand())
                {
                    lockCommand.CommandText = @"
                        UPDATE Cards
                        SET IsLocked = TRUE
                        WHERE Id = @CardId;
                    ";
                    lockCommand.Parameters.AddWithValue("@CardId", trade.TradedCard.Id);
                    if (lockCommand.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("Failed to lock the card for trading");
                    }
                }

                // add trade to the DB
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Trades (Id, TradedCardId, WantedElement, WantedTribe, WantedMinDamage)
                        VALUES (@Id, @TradedCardId, @WantedElement, @WantedTribe, @WantedMinDamage);
                    ";
                    command.Parameters.AddWithValue("@Id", trade.Id);
                    command.Parameters.AddWithValue("TradedCardId", trade.TradedCard.Id);
                    command.Parameters.AddWithValue("@WantedElement", (int)trade.WantedElement);
                    command.Parameters.AddWithValue("@WantedTribe", (int)trade.WantedTribe);
                    command.Parameters.AddWithValue("@WantedMinDamage", trade.WantedMinDamage);

                    command.ExecuteNonQuery();
                }

                // finalize transaction
                transaction.Commit();
            }
            catch
            {
                // in case of error
                transaction.Rollback();
                throw;
            }
        }

        public Trading? GetTradeById(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Id, c.Id, c.Name, c.Damage, c.Element, c.Tribe, c.CardType, c.OwnerId, c.IsLocked, 
                       t.WantedElement, t.WantedTribe, t.WantedMinDamage
                FROM Trades t
                JOIN Cards c ON t.TradedCardId = c.Id
                WHERE t.Id = @Id;
            ";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var cardType = (CardType)reader.GetInt32(6);
                var ownerId = reader.GetGuid(7);

                Card card = cardType switch
                {
                    CardType.Monster => new MonsterCard(
                        reader.GetString(2),            // Name
                        reader.GetDouble(3),            // Damage
                        (Element)reader.GetInt32(4),    // Element
                        (Tribe)reader.GetInt32(5),      // Tribe
                        ownerId                         // OwnerId
                    )
                    {
                        Id = reader.GetGuid(1),         // Card ID
                        IsLocked = reader.GetBoolean(8) // IsLocked
                    },
                    CardType.Spell => new SpellCard(
                        reader.GetString(2),            // Name
                        reader.GetDouble(3),            // Damage
                        (Element)reader.GetInt32(4),    // Element
                        ownerId                         // OwnerId
                    )
                    {
                        Id = reader.GetGuid(1),         // Card ID
                        IsLocked = reader.GetBoolean(8) // IsLocked
                    },
                    _ => throw new InvalidOperationException("Unknown card type")
                };

                return new Trading(
                    reader.GetGuid(0),                  // Trade ID
                    card,
                    (Element)reader.GetInt32(9),        // WantedElement
                    (Tribe)reader.GetInt32(10),         // WantedTribe
                    reader.GetFloat(11)                 // WantedMinDamage
                );
            }

            return null;
        }

        public void DeleteTrade(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // retrieve card before deleting trade
                var trade = GetTradeById(id);
                if (trade == null || trade?.TradedCardId == null)
                {
                    throw new InvalidOperationException("Trade not found");
                }

                // unlock card
                using var unlockCommand = connection.CreateCommand();
                unlockCommand.CommandText = @"
                    UPDATE Cards
                    SET IsLocked = FALSE
                    WHERE Id = @CardId;
                ";
                unlockCommand.Parameters.AddWithValue("@CardId", trade.TradedCardId);
                unlockCommand.ExecuteNonQuery();

                // delete the trade
                using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = @"
                    DELETE FROM Trades
                    WHERE Id = @Id;
                ";
                deleteCommand.Parameters.AddWithValue("@Id", id);
                deleteCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Trading> GetTrades()
        {
            var trades = new List<Trading>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Id, c.Id, c.Name, c.Damage, c.Element, c.Tribe, c.CardType, c.OwnerId, c.IsLocked, 
                       t.WantedElement, t.WantedTribe, t.WantedMinDamage
                FROM Trades t
                JOIN Cards c ON t.TradedCardId = c.Id;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var cardType = (CardType)reader.GetInt32(6);
                var ownerId = reader.GetGuid(7);

                Card card = cardType switch
                {
                    CardType.Monster => new MonsterCard(
                        reader.GetString(2),            // Name
                        reader.GetDouble(3),            // Damage
                        (Element)reader.GetInt32(4),    // Element
                        (Tribe)reader.GetInt32(5),      // Tribe
                        ownerId                         // OwnerId
                    )
                    {
                        Id = reader.GetGuid(1),         // Card ID
                        IsLocked = reader.GetBoolean(8) // IsLocked
                    },
                    CardType.Spell => new SpellCard(
                        reader.GetString(2),            // Name
                        reader.GetDouble(3),            // Damage
                        (Element)reader.GetInt32(4),    // Element
                        ownerId                         // OwnerId
                    )
                    {
                        Id = reader.GetGuid(1),         // Card ID
                        IsLocked = reader.GetBoolean(8) // IsLocked
                    },
                    _ => throw new InvalidOperationException("Unknown card type")
                };

                trades.Add(new Trading(
                    reader.GetGuid(0),                  // Trade ID
                    card,
                    (Element)reader.GetInt32(9),        // WantedElement
                    (Tribe)reader.GetInt32(10),         // WantedTribe
                    reader.GetFloat(11)                 // WantedMinDamage
                ));
            }

            return trades;
        }


    }
}
