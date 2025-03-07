using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    internal class UserHandler
    {
        private readonly IUserRepo _userRepo;

        public UserHandler(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public bool RegisterUser(string username, string password)
        {
            return _userRepo.RegisterUser(username, password);
        }

        public string? LoginUser(string username, string password)
        {
            var user = _userRepo.LoginUser(username, password);
            return user?.Token;
        }

        public User? FindUserByToken(string token)
        {
            return _userRepo.GetUserByToken(token);
        }

        public List<Card> GetDeckByToken(string token)
        {
            if (token != null)
            {
                var user = FindUserByToken(token);
                if (user != null)
                {
                    // Check if user deck exists
                    if (user.Deck != null)
                    {
                        return user.Deck.Cards;
                    }
                }
            }
            // returns empty deck if anything went wrong
            return new List<Card>();
        }

        public User? GetUserByUsername(string username)
        {
            var users = _userRepo.GetUsersStartingWith(username);

            if (users.Count == 1)    // Exactly one user found
            {
                return users[0];
            }

            Console.WriteLine(users.Count == 0
                ? $"No user found with the username {username}"                             // no hits
                : $"Username {username} not found. Found {users.Count} similar matches");   // multiple hits
            return null;
        }

        public void UpdateUser(User user)
        {
            _userRepo.UpdateUser(user);
        }

        public Stats? GetStatsByToken(string token)
        {
            var user = FindUserByToken(token);
            return user?.Stats;  // null if user not found
        }

        // get user stats from scoreboard ordered by elo
        public List<Stats> GetScoreboard()
        {
            return _userRepo.GetUsers()
                .OrderByDescending(u => u.Stats.Elo)
                .Select(u => u.Stats)
                .ToList();
        }

        // Update user elo + W/L after a battle
        public void UpdateUserElo(string token, int points, bool won)
        {
            var user = FindUserByToken(token);
            if (user != null)
            {
                user.Stats.Elo += points;
                if (won)
                {
                    user.Stats.Wins++;
                }
                else
                {
                    user.Stats.Losses++;
                }

                UpdateUser(user);
            }
        }

        public void AddCardToDeck(User user, Card card)
        {
            if (user.Deck.Cards.Count < 4)
            {
                user.Deck.AddCard(card);
            }
            else
            {
                throw new InvalidOperationException("The deck can only contain 4 cards");
            }
        }

        public void RemoveCardFromDeck(User user, Card card)
        {
            user.Deck.RemoveCard(card);
        }

        public void AddCardToStack(User user, Card card)
        {
            user.Stack.AddCard(card);
        }

        public Card DrawCardFromStack(User user)
        {
            return user.Stack.DrawCard();
        }

        public bool SpendCoins(User user, int amount)
        {
            if (user.Coins >= amount)   // check if user has enough coins (pack = 5 coins)
            {
                user.Coins -= amount;
                _userRepo.UpdateUser(user);
                return true;
            }
            return false;   // Too poor, boo-hoo, just get some coins mate
        }

    }
}
