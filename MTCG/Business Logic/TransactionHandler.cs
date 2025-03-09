using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    public class TransactionHandler
    {
        private readonly UserHandler _userHandler;
        private readonly IPackRepo _packRepo;
        private const int packCost = 5;

        public TransactionHandler(UserHandler userHandler, IPackRepo packRepo)
        {
            _userHandler = userHandler;
            _packRepo = packRepo;
        }

        public (bool IsSuccessful, List<Card>? BoughtCards, string? ErrorMessage) BuyPack(string token)
        {
            // validate user by token
            var user = _userHandler.FindUserByToken(token);
            if (user == null)
            {
                return (false, null, "Invalid auth token");
            }

            // check if user has enough coins
            if (user.Coins < packCost)
            {
                return (false, null, "Not enough money to buy the pack");
            }

            // check if packs are available
            List<Card>? cards = _packRepo.GetAvailablePack();
            if (cards == null)
            {
                return (false, null, "No packages available for purchase");
            }

            // add all cards from package to user stack in DB
            Console.WriteLine($"Card currently owned by {cards.First().OwnerId}");
            var transferSuccess = _packRepo.TransferOwnership(cards, user.Id);
            if (!transferSuccess)
            {
                return (false, null, "Failed to transfer pack ownership");
            }
            Console.WriteLine($"Transfer successful! Card now owned by {cards.First().OwnerId}");

            // subtract coin cost from user
            if (!_userHandler.SpendCoins(user, packCost))
            {
                return (false, null, "Not enough money to buy the package"); // Too poor
            }

            // pack return successful
            return (true, cards, null);
        }
    }
}
