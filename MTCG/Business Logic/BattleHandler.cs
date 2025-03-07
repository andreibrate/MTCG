using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    public class BattleHandler
    {
        private readonly IUserRepo _userRepo;
        private readonly IDeckRepo _deckRepo;
        private readonly ICardRepo _cardRepo;
        private readonly BattleSpecialConditions _specialConditions;

        public BattleHandler(IUserRepo userRepo, IDeckRepo deckRepo, ICardRepo cardRepo)
        {
            _userRepo = userRepo;
            _deckRepo = deckRepo;
            _cardRepo = cardRepo;
            _specialConditions = new BattleSpecialConditions();
        }

        public (string Winner, string Loser) StartBattle(string player1Token, string player2Token)
        {
            var battleLog = new List<string>(); // store logs of each round
            var defeatedCardsPlayer1 = new List<Card>();
            var defeatedCardsPlayer2 = new List<Card>();

            try
            {
                var player1 = _userRepo.GetUserByToken(player1Token);
                var player2 = _userRepo.GetUserByToken(player2Token);

                if (player1 == null || player2 == null)
                {
                    throw new InvalidOperationException("One or both players are invalid");
                }

                var deck1 = new Deck { Cards = _deckRepo.GetDeckByUserId(player1.Id).ToList() };
                var deck2 = new Deck { Cards = _deckRepo.GetDeckByUserId(player2.Id).ToList() };

                if (deck1.Cards.Count != 4 || deck2.Cards.Count != 4)
                {
                    throw new InvalidOperationException("Both players must have exactly 4 cards in their decks");
                }

                int score1 = 0, score2 = 0;

                for (int round = 0; round < 4; round++) // 4 rounds
                {
                    // draw a card from each deck
                    var card1 = deck1.DrawCard(); // player 1 draws
                    var card2 = deck2.DrawCard(); // player 2 draws

                    int result = _specialConditions.DecideWinner(card1, card2);
                    if (result > 0)
                    {
                        score1++;
                        defeatedCardsPlayer2.Add(card2);
                        battleLog.Add($"Round {round}: {card1.Name} (Player 1) defeated {card2.Name} (Player 2).");
                    }
                    else if (result < 0)
                    {
                        score2++;
                        defeatedCardsPlayer1.Add(card1);
                        battleLog.Add($"Round {round}: {card2.Name} (Player 2) defeated {card1.Name} (Player 1).");
                    }
                    else
                    {
                        battleLog.Add($"Round {round}: {card1.Name} (Player 1) tied with {card2.Name} (Player 2).");
                    }
                }

                string winner, loser;
                if (score1 > score2)
                {
                    winner = player1.Username;
                    loser = player2.Username;
                    UpdateStats(player1, player2, true);
                }
                else if (score2 > score1)
                {
                    winner = player2.Username;
                    loser = player1.Username;
                    UpdateStats(player2, player1, true);
                }
                else
                {
                    winner = "Draw";
                    loser = "Draw";
                    UpdateStats(player1, player2, false);
                }

                // add battle winner + loser to log
                battleLog.Add($"Battle Result: Winner - {winner}, Loser - {loser}");

                return (winner, loser);
            }
            catch (Exception ex)
            {
                battleLog.Add($"Error: {ex.Message}"); // exception for debugging
                Console.WriteLine(string.Join(Environment.NewLine, battleLog));
                throw;  // rethrow
            }
        }

        private void UpdateStats(User winner, User loser, bool hasWinner)
        {
            const int eloWinChange = 3;
            const int eloLossChange = -5;
            const int eloDrawChange = 1;
            const int loserCoins = 5;
            const int winnerCoins = 10;

            if (hasWinner)
            {
                winner.Stats.Wins++;
                winner.Stats.Elo += eloWinChange;

                loser.Stats.Losses++;
                loser.Stats.Elo += eloLossChange;
            }
            else
            {
                // draw => both players get 1 elo
                winner.Stats.Elo += eloDrawChange;
                loser.Stats.Elo += eloDrawChange;
            }

            // ensure both players get at least 5 coins (min for participating)
            winner.Coins += winnerCoins;
            loser.Coins += loserCoins;
            Console.WriteLine($"{winner.Username} earned {winner.Coins} coins");
            Console.WriteLine($"{loser.Username} earned {loser.Coins} coins");

            _userRepo.UpdateUser(winner);
            _userRepo.UpdateUser(loser);
        }


    }
}
