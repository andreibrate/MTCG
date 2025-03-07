using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    public class TradeHandler
    {
        private readonly ITradeRepo _tradeRepo;
        private readonly ICardRepo _cardRepo;
        private readonly IUserRepo _userRepo;

        public TradeHandler(ITradeRepo tradeRepo, ICardRepo cardRepo, IUserRepo userRepo)
        {
            _tradeRepo = tradeRepo;
            _cardRepo = cardRepo;
            _userRepo = userRepo;
        }

        public void CreateTrade(Trading trade)
        {
            if (trade.TradedCardId == null)
                throw new ArgumentException("TradedCardId can't be null");

            var tradedCard = _cardRepo.GetCardById(trade.TradedCardId.Value);
            if (tradedCard == null)
                throw new InvalidOperationException("The card doesn't exist");

            if (tradedCard.IsLocked)
                throw new InvalidOperationException("The card to trade is locked and cannot be offered");

            // create trade object
            trade.TradedCard = tradedCard;

            _tradeRepo.AddTrade(trade);
        }

        public bool AcceptTrade(Guid tradeId, Guid offeredCardId)
        {
            var trade = _tradeRepo.GetTradeById(tradeId);
            if (trade == null || trade.TradedCardId == null)
                throw new InvalidOperationException("Trade not found");

            var offeredCard = _cardRepo.GetCardById(offeredCardId);
            if (offeredCard == null || offeredCard.IsLocked)
                throw new InvalidOperationException("The offered card is invalid or locked.");

            if (offeredCard.Damage < trade.WantedMinDamage)
            {
                return false; // condition not met
            }

            // perform trade
            var offeredCardOwnerId = offeredCard.OwnerId;
            offeredCard.OwnerId = trade.TradedCard?.OwnerId ?? throw new InvalidOperationException("Invalid trade state");
            trade.TradedCard.OwnerId = offeredCardOwnerId;

            // lock traded cards after the trade (to prevent immediate re-trade)
            offeredCard.IsLocked = true;
            trade.TradedCard.IsLocked = true;

            // update cards in DB
            _cardRepo.UpdateCard(offeredCard);
            _cardRepo.UpdateCard(trade.TradedCard);

            // delete the trade after completion
            _tradeRepo.DeleteTrade(tradeId);

            return true;
        }

        public List<Trading> GetTrades()
        {
            return _tradeRepo.GetTrades();
        }

        public Trading? GetTradeById(Guid id)
        {
            return _tradeRepo.GetTradeById(id);
        }

        public void DeleteTrade(Guid id)
        {
            _tradeRepo.DeleteTrade(id);
        }

        public Card? GetCardById(Guid cardId)
        {
            return _cardRepo.GetCardById(cardId);
        }

        public User? GetUserByToken(string token)
        {
            return _userRepo.GetUserByToken(token);
        }
    }
}
