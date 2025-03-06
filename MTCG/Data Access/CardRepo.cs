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
    public class CardRepo : ICardRepo
    {
        private readonly string _connectionString;
        public CardRepo(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<Card> GetCardsByUserId(Guid userId)
        {
            var cards = new List<Card>();

            if (userId == Guid.Empty)
            {
                Console.WriteLine($"userId {userId} does not exist");
                throw new ArgumentException("Invalid UserId");
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, Name, Damage, Element, Tribe, CardType, OwnerId, IsLocked
                        FROM Cards
                        WHERE OwnerId = @UserId;
                    ";
                    command.Parameters.AddWithValue("UserId", userId);

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
                            var isLocked = reader.GetBoolean(7);

                            Console.WriteLine($"Card found: ID={id}, OwnerID={ownerId}, Name={name}");

                            if (cardType == CardType.Monster)
                            {
                                var tribe = (Tribe)reader.GetInt32(4);
                                var monsterCard = new MonsterCard(name, damage, element, tribe, ownerId) { IsLocked = isLocked, Id = id };
                                cards.Add(monsterCard);
                            }
                            else
                            {
                                var spellCard = new SpellCard(name, damage, element, ownerId) { IsLocked = isLocked, Id = id };
                                cards.Add(spellCard);
                            }
                        }
                    }
                    return cards;
                }
            }
        }






        public Card? GetCardById(Guid cardId)
        {
            throw new NotImplementedException();
        }
        public bool AddCard(Card card)
        {
            throw new NotImplementedException();
        }
        public void UpdateCard(Card card)
        {
            throw new NotImplementedException();
        }
    }
}
