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
    public class DeckRepo : IDeckRepo
    {
        private readonly string _connectionString;
        public DeckRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Card> GetDeckByUserId(Guid userId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT c.Id, c.Name, c.Damage, c.Element, c.Tribe, c.CardType
                FROM Cards c
                JOIN Decks d ON c.Id = ANY(d.CardIds)
                WHERE d.UserId = @UserId;
            ";
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();
            var cards = new List<Card>();
            while (reader.Read())
            {
                var id = reader.GetGuid(0);
                var name = reader.GetString(1);
                var damage = reader.GetDouble(2);
                var element = (Element)reader.GetInt32(3);
                var cardType = (CardType)reader.GetInt32(5);

                if (cardType == CardType.Monster)
                {
                    var tribe = (Tribe)reader.GetInt32(4);
                    cards.Add(new MonsterCard(name, damage, element, tribe, userId) { Id = id });
                }
                else
                {
                    cards.Add(new SpellCard(name, damage, element, userId) { Id = id });
                }
                Console.WriteLine($"Card found: ID={id}, OwnerID={userId}, Name={name}");
            }
            if (!cards.Any())
            {
                Console.WriteLine($"No cards found in the deck for user with OwnerID={userId}.");
            }
            return cards;
        }

        public bool AddCardToDeck(Guid userId, IEnumerable<Guid> cardIds)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Decks (UserId, CardIds)
                VALUES (@UserId, @CardIds)
                ON CONFLICT (UserId) DO UPDATE
                SET CardIds = @CardIds;
            ";
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@CardIds", cardIds.ToArray());

            return command.ExecuteNonQuery() > 0;
        }

        public bool RemoveCardsFromDeck(Guid userId, IEnumerable<Guid> cardIds)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT CardIds
                FROM Decks
                WHERE UserId = @UserId;
            ";
            command.Parameters.AddWithValue("@UserId", userId);

            var currentCardIds = new List<Guid>();
            using var reader = command.ExecuteReader();
            {
                if (reader.Read() && !reader.IsDBNull(0))
                {
                    currentCardIds = reader.GetFieldValue<Guid[]>(0).ToList();
                }
            }

            var newCardIds = currentCardIds.Except(cardIds).ToArray(); // determine cards that were removed
            using var updateCommand = connection.CreateCommand(); // update the deck with the new card ids
            updateCommand.CommandText = @"
                UPDATE Decks
                SET CardIds = @NewCardIds
                WHERE UserId = @UserId;
            ";

            command.Parameters.AddWithValue("@NewCardIds", newCardIds);
            command.Parameters.AddWithValue("@UserId", userId);

            return command.ExecuteNonQuery() > 0;
        }
    }
}
