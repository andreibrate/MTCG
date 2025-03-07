using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Enums;
using MTCG.Models;
using Npgsql;
using MTCG.Data_Access;

namespace MTCG.Test.Data_Access
{
    [TestFixture]
    public class CardRepoTests
    {
        private CardRepo _cardRepo;
        private UserRepo _userRepo;
        private string _connectionString;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;

        [SetUp]
        public void Setup()
        {
            _connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mtcgdb";
            DbManager.CleanupTables(_connectionString);
            DbManager.InitializeDatabase(_connectionString);
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
            _cardRepo = new CardRepo(_connectionString);
            _userRepo = new UserRepo(_connectionString);
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                _transaction.Rollback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error during Transaction Rollback: {ex.Message}");
            }
            finally
            {
                DbManager.CleanupTables(_connectionString);
                _transaction?.Dispose();
                _connection?.Dispose();
            }
        }

        private User CreateTestUser(string username, string password)
        {
            var isRegistered = _userRepo.RegisterUser(username, password);
            Assert.That(isRegistered, Is.True, "User registration failed");
            var user = _userRepo.LoginUser(username, password);
            Assert.That(user, Is.Not.Null, "User login failed");
            return user!;
        }

        [Test]
        public void AddCard_ValidCard_SuccessfullyAddsCard()
        {
            var user = CreateTestUser("testuser", "password123");
            var card = new MonsterCard("Dragon", 50, Element.Fire, Tribe.Dragon, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = false
            };

            Assert.DoesNotThrow(() => _cardRepo.AddCard(card));

            var retrievedCard = _cardRepo.GetCardById(card.Id);
            Assert.That(retrievedCard, Is.Not.Null);
            Assert.That(retrievedCard?.Name, Is.EqualTo(card.Name));
            Assert.That(retrievedCard?.IsLocked, Is.False);
        }

        [Test]
        public void AddCard_SpellCard_SuccessfullyAddsCard()
        {
            var user = CreateTestUser("testuser4", "password123");
            var card = new SpellCard("Fireball", 40, Element.Fire, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = true
            };

            Assert.DoesNotThrow(() => _cardRepo.AddCard(card));

            var retrievedCard = _cardRepo.GetCardById(card.Id);
            Assert.That(retrievedCard, Is.Not.Null);
            Assert.That(retrievedCard?.Name, Is.EqualTo(card.Name));
            Assert.That(retrievedCard?.IsLocked, Is.True);
        }

        [Test]
        public void GetCardById_ValidId_ReturnsCorrectCard()
        {
            var user = CreateTestUser("testuser2", "password123");
            var card = new MonsterCard("Elf", 30, Element.Water, Tribe.Elf, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = false
            };
            _cardRepo.AddCard(card);

            var retrievedCard = _cardRepo.GetCardById(card.Id);

            Assert.That(retrievedCard, Is.Not.Null);
            Assert.That(retrievedCard?.Id, Is.EqualTo(card.Id));
            Assert.That(retrievedCard?.Name, Is.EqualTo(card.Name));
        }

        [Test]
        public void GetCardById_MonsterCard_ReturnsCorrectCard()
        {
            // Arrange
            _userRepo.RegisterUser("testuser5", "password123");
            var user = _userRepo.LoginUser("testuser5", "password123");
            Assert.That(user, Is.Not.Null, "User should be successfully logged in");

            var card = new MonsterCard("Ork", 60, Element.Normal, Tribe.Ork, user!.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = true
            };
            _cardRepo.AddCard(card);

            // Act
            var retrievedCard = _cardRepo.GetCardById(card.Id);

            // Assert
            Assert.That(retrievedCard, Is.Not.Null, "Retrieved card should not be null");
            Assert.That(retrievedCard, Is.InstanceOf<MonsterCard>(), "Retrieved card should be a MonsterCard");
            Assert.That(retrievedCard?.Id, Is.EqualTo(card.Id), "Card ID should match");
            Assert.That(retrievedCard?.Name, Is.EqualTo(card.Name), "Card Name should match");
            Assert.That(retrievedCard?.Damage, Is.EqualTo(card.Damage), "Card Damage should match");
            Assert.That(retrievedCard?.Element, Is.EqualTo(card.Element), "Card Element should match");
            Assert.That(((MonsterCard)retrievedCard!).Tribe, Is.EqualTo(Tribe.Ork), "Tribe should match");
            Assert.That(retrievedCard?.IsLocked, Is.EqualTo(card.IsLocked), "IsLocked status should match");
        }

        [Test]
        public void GetCardsByUserId_ValidUserId_ReturnsCards()
        {
            var user = CreateTestUser("testuser3", "password123");
            var card1 = new MonsterCard("Goblin", 20, Element.Normal, Tribe.Goblin, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = false
            };
            var card2 = new MonsterCard("Elf", 40, Element.Water, Tribe.Elf, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = true
            };
            _cardRepo.AddCard(card1);
            _cardRepo.AddCard(card2);

            var userCards = _cardRepo.GetCardsByUserId(user.Id);

            Assert.That(userCards.Count, Is.EqualTo(2));
            Assert.That(userCards, Has.Exactly(1).Matches<Card>(c => c.Id == card1.Id));
            Assert.That(userCards, Has.Exactly(1).Matches<Card>(c => c.Id == card2.Id));
        }

        [Test]
        public void GetCardsByUserId_ReturnsAllCardTypes()
        {
            var user = CreateTestUser("testuser6", "password123");
            var card1 = new MonsterCard("Dragon", 50, Element.Fire, Tribe.Dragon, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = false
            };
            var card2 = new SpellCard("Fireball", 40, Element.Fire, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = true
            };
            _cardRepo.AddCard(card1);
            _cardRepo.AddCard(card2);

            var userCards = _cardRepo.GetCardsByUserId(user.Id);

            Assert.That(userCards.Count, Is.EqualTo(2));
            Assert.That(userCards, Has.Exactly(1).Matches<Card>(c => c is MonsterCard && c.Id == card1.Id));
            Assert.That(userCards, Has.Exactly(1).Matches<Card>(c => c is SpellCard && c.Id == card2.Id));
        }

        [Test]
        public void GetCardById_LockedCard_ReturnsCardWithCorrectLockStatus()
        {
            var user = CreateTestUser("testuser8", "password123");
            var card = new SpellCard("Shield", 25, Element.Water, user.Id)
            {
                Id = Guid.NewGuid(),
                IsLocked = true
            };
            _cardRepo.AddCard(card);

            var retrievedCard = _cardRepo.GetCardById(card.Id);

            Assert.That(retrievedCard, Is.Not.Null);
            Assert.That(retrievedCard?.IsLocked, Is.True);
        }
    }
}
