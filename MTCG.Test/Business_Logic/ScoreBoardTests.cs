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
    public class ScoreboardHandlerTests
    {
        private IUserRepo _userRepo;
        private ScoreboardHandler _scoreboardHandler;

        [SetUp]
        public void Setup()
        {
            _userRepo = Substitute.For<IUserRepo>();
            _scoreboardHandler = new ScoreboardHandler(_userRepo);
        }

        [Test]
        public void GetScoreboard_ReturnsSortedUserStats()
        {
            // Arrange
            var scoreboardData = new List<(string Username, Stats Stats)>
            {
                ("Player1", new Stats { Elo = 1200, Wins = 10, Losses = 5 }),
                ("Player2", new Stats { Elo = 1300, Wins = 15, Losses = 3 }),
                ("Player3", new Stats { Elo = 1100, Wins = 8, Losses = 7 })
            };

            _userRepo.GetScoreboardStats().Returns(scoreboardData);

            // Act
            var result = _scoreboardHandler.GetScoreboard();

            // Assert
            Assert.That(result, Is.EqualTo(scoreboardData));
        }

        [Test]
        public void GetScoreboard_NoUsers_ReturnsEmptyList()
        {
            // Arrange
            _userRepo.GetScoreboardStats().Returns(new List<(string Username, Stats Stats)>());

            // Act
            var result = _scoreboardHandler.GetScoreboard();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetScoreboard_HandlesNullFromRepository()
        {
            // Arrange
            _userRepo.GetScoreboardStats().Returns((IEnumerable<(string Username, Stats Stats)>?)null);

            // Act
            var result = _scoreboardHandler.GetScoreboard() ?? Enumerable.Empty<(string Username, Stats Stats)>();

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
