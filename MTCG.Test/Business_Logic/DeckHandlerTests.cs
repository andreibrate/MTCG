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
    public class DeckHandlerTests
    {
        private IDeckRepo _deckRepo;
        private ICardRepo _cardRepo;
        private DeckHandler _deckHandler;

        [SetUp]
        public void Setup()
        {
            _deckRepo = Substitute.For<IDeckRepo>();
            _cardRepo = Substitute.For<ICardRepo>();
            _deckHandler = new DeckHandler(_deckRepo, _cardRepo);
        }

        [Test]
        public void GetDeckByUserId_ValidUserId_ReturnsDeck()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedDeck = new List<Card>
            {
                new MonsterCard("Dragon", 50, Enums.Element.Fire, Enums.Tribe.Dragon, userId),
                new SpellCard("Fireball", 40, Enums.Element.Fire, userId),
                new MonsterCard("Elf", 25, Enums.Element.Normal, Enums.Tribe.Elf, userId),
                new SpellCard("WaterSplash", 30, Enums.Element.Water, userId)
            };

            _deckRepo.GetDeckByUserId(userId).Returns(expectedDeck);

            // Act
            var result = _deckHandler.GetDeckByUserId(userId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDeck));
        }

        [Test]
        public void GetDeckByUserId_InvalidUserId_ReturnsEmptyDeck()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _deckRepo.GetDeckByUserId(userId).Returns(new List<Card>());

            // Act
            var result = _deckHandler.GetDeckByUserId(userId);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ValidateDeck_IsDeckValid_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cardIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var userCards = new List<Card>
            {
                new MonsterCard("Dragon", 50, Enums.Element.Fire, Enums.Tribe.Dragon, userId) { Id = cardIds[0] },
                new SpellCard("Fireball", 40, Enums.Element.Fire, userId) { Id = cardIds[1] },
                new MonsterCard("Elf", 25, Enums.Element.Normal, Enums.Tribe.Elf, userId) { Id = cardIds[2] },
                new SpellCard("WaterSplash", 30, Enums.Element.Water, userId) { Id = cardIds[3] }
            };

            _cardRepo.GetCardsByUserId(userId).Returns(userCards);

            // Act
            var result = _deckHandler.IsDeckValid(cardIds, userId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateDeck_InvalidDeckSize_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cardIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }; // Only 2 cards

            // Act
            var result = _deckHandler.IsDeckValid(cardIds, userId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateDeck_InvalidOwnership_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cardIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var userCards = new List<Card>
            {
                new MonsterCard("Dragon", 50, Enums.Element.Fire, Enums.Tribe.Dragon, userId) { Id = Guid.NewGuid() },
                new SpellCard("Fireball", 40, Enums.Element.Fire, userId) { Id = Guid.NewGuid() }
            };

            _cardRepo.GetCardsByUserId(userId).Returns(userCards);

            // Act
            var result = _deckHandler.IsDeckValid(cardIds, userId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateDeck_IsDeckValid_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cardIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var userCards = new List<Card>
            {
                new MonsterCard("Dragon", 50, Enums.Element.Fire, Enums.Tribe.Dragon, userId) { Id = cardIds[0] },
                new SpellCard("Fireball", 40, Enums.Element.Fire, userId) { Id = cardIds[1] },
                new MonsterCard("Elf", 25, Enums.Element.Normal, Enums.Tribe.Elf, userId) { Id = cardIds[2] },
                new SpellCard("WaterSplash", 30, Enums.Element.Water, userId) { Id = cardIds[3] }
            };

            _cardRepo.GetCardsByUserId(userId).Returns(userCards);
            _deckRepo.UpdateDeck(userId, cardIds).Returns(true);

            // Act
            var result = _deckHandler.UpdateDeck(userId, cardIds);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UpdateDeck_InvalidDeck_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cardIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            // Mock invalid validation
            _cardRepo.GetCardsByUserId(userId).Returns(new List<Card>());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _deckHandler.UpdateDeck(userId, cardIds));
        }
    }
}
