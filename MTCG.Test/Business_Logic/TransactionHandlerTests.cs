using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Business_Logic;
using NSubstitute;
using MTCG.Models;
using MTCG.Enums;

namespace MTCG.Test.Business_Logic
{
    [TestFixture]
    public class TransactionHandlerTests
    {
        private IUserRepo _userRepo;
        private IPackRepo _packRepo;
        private UserHandler _userHandler;
        private TransactionHandler _transactionHandler;

        [SetUp]
        public void Setup()
        {
            _userRepo = Substitute.For<IUserRepo>();
            _packRepo = Substitute.For<IPackRepo>();
            _userHandler = new UserHandler(_userRepo); // real UserHandler with mocked IUserRepo
            _transactionHandler = new TransactionHandler(_userHandler, _packRepo);
        }

        [Test]
        public void BuyPack_UserNotFound_ReturnsError()
        {
            // Arrange
            var token = "invalid-token";
            _userRepo.GetUserByToken(token).Returns((User)null!);

            // Act
            var result = _transactionHandler.BuyPack(token);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid authentication token"));
        }

        [Test]
        public void BuyPack_NotEnoughCoins_ReturnsError()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Coins = 3 };
            var token = "valid-token";
            _userRepo.GetUserByToken(token).Returns(user);

            // Act
            var result = _transactionHandler.BuyPack(token);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not enough money to buy the package"));
        }

        [Test]
        public void BuyPack_NoPacksAvailable_ReturnsError()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Coins = 10 };
            var token = "valid-token";
            _userRepo.GetUserByToken(token).Returns(user);
            _packRepo.GetAvailablePack().Returns((List<Card>)null!);

            // Act
            var result = _transactionHandler.BuyPack(token);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("No packages available for purchase"));
        }

        [Test]
        public void BuyPack_FailedOwnershipTransfer_ReturnsError()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Coins = 10 };
            var token = "valid-token";
            var cards = new List<Card>
            {
                new MonsterCard("Dragon", 50, Element.Fire, Tribe.Dragon, Guid.Empty),
                new MonsterCard("Goblin", 20, Element.Normal, Tribe.Goblin, Guid.Empty)
            };

            _userRepo.GetUserByToken(token).Returns(user);
            _packRepo.GetAvailablePack().Returns(cards);
            _packRepo.TransferOwnership(cards, user.Id).Returns(false);

            // Act
            var result = _transactionHandler.BuyPack(token);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Failed to transfer ownership of the package"));
        }

        [Test]
        public void BuyPack_SuccessfullyBuysPack_ReturnsPackage()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Coins = 10, Stack = new Stack() };
            var token = "valid-token";
            var cards = new List<Card>
            {
                new MonsterCard("Dragon", 50, Element.Fire, Tribe.Dragon, user.Id),
                new MonsterCard("Goblin", 20, Element.Normal, Tribe.Goblin, user.Id)
            };

            _userRepo.GetUserByToken(token).Returns(user);
            _packRepo.GetAvailablePack().Returns(cards);
            _packRepo.TransferOwnership(cards, user.Id).Returns(true);

            // Act
            var result = _transactionHandler.BuyPack(token);

            // Assert
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.BoughtCards, Is.EqualTo(cards));
            Assert.That(user.Coins, Is.EqualTo(5)); // Verify coins deduction
        }
    }
}
