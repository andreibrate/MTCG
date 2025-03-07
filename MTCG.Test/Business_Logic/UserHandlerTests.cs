using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Business_Logic;
using NSubstitute;
using MTCG.Models;

namespace MTCG.Test.Business_Logic
{
    [TestFixture]
    public class UserHandlerTests
    {
        private IUserRepo _userRepo;
        private UserHandler _userHandler;

        [SetUp]
        public void Setup()
        {
            _userRepo = Substitute.For<IUserRepo>();
            _userHandler = new UserHandler(_userRepo);
        }

        [Test]
        public void RegisterUser_ValidUser_ReturnsTrue()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            _userRepo.RegisterUser(username, password).Returns(true);

            // Act
            var result = _userHandler.RegisterUser(username, password);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void RegisterUser_DuplicateUser_ReturnsFalse()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            _userRepo.RegisterUser(username, password).Returns(false);

            // Act
            var result = _userHandler.RegisterUser(username, password);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void LoginUser_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var expectedToken = "valid-token";
            _userRepo.LoginUser(username, password).Returns(new User { Id = Guid.NewGuid(), Token = expectedToken });

            // Act
            var result = _userHandler.LoginUser(username, password);

            // Assert
            Assert.That(result, Is.EqualTo(expectedToken));
        }

        [Test]
        public void LoginUser_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            _userRepo.LoginUser(username, password).Returns((User)null!);

            // Act
            var result = _userHandler.LoginUser(username, password);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetDeckByToken_ValidToken_ReturnsDeck()
        {
            // Arrange
            var token = "valid-token";
            var userId = Guid.NewGuid();
            var expectedDeck = new List<Card>
            {
                new MonsterCard("Dragon", 50, Enums.Element.Fire, Enums.Tribe.Dragon, userId)
            };
            var user = new User { Id = userId, Deck = new Deck { Cards = expectedDeck } };
            _userRepo.GetUserByToken(token).Returns(user);

            // Act
            var result = _userHandler.GetDeckByToken(token);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDeck));
        }

        [Test]
        public void GetDeckByToken_InvalidToken_ReturnsEmptyList()
        {
            // Arrange
            var token = "invalid-token";
            _userRepo.GetUserByToken(token).Returns((User)null!);

            // Act
            var result = _userHandler.GetDeckByToken(token);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void UpdateUserElo_Win_UpdatesStatsCorrectly()
        {
            // Arrange
            var token = "valid-token";
            var user = new User { Id = Guid.NewGuid(), Stats = new Stats { Elo = 1000, Wins = 10 } };
            _userRepo.GetUserByToken(token).Returns(user);

            // Act
            _userHandler.UpdateUserElo(token, 50, true);

            // Assert
            Assert.That(user.Stats.Elo, Is.EqualTo(1050));
            Assert.That(user.Stats.Wins, Is.EqualTo(11));
        }

        [Test]
        public void UpdateUserElo_Loss_UpdatesStatsCorrectly()
        {
            // Arrange
            var token = "valid-token";
            var user = new User { Id = Guid.NewGuid(), Stats = new Stats { Elo = 1000, Losses = 5 } };
            _userRepo.GetUserByToken(token).Returns(user);

            // Act
            _userHandler.UpdateUserElo(token, -30, false);

            // Assert
            Assert.That(user.Stats.Elo, Is.EqualTo(970));
            Assert.That(user.Stats.Losses, Is.EqualTo(6));
        }

        [Test]
        public void SpendCoins_UserHasEnoughCoins_ReturnsTrue()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Coins = 10 };

            // Act
            var result = _userHandler.SpendCoins(user, 5);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(user.Coins, Is.EqualTo(5));
        }

        [Test]
        public void SpendCoins_UserNotEnoughCoins_ReturnsFalse()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Coins = 3 };

            // Act
            var result = _userHandler.SpendCoins(user, 5);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(user.Coins, Is.EqualTo(3));
        }
    }
}
