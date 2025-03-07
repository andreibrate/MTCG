using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;
using MTCG.Data_Access.Interfaces;


namespace MTCG.Business_Logic
{
    internal class CardHandler
    {
        private readonly ICardRepo _cardRepo;

        public CardHandler(ICardRepo cardRepository)
        {
            _cardRepo = cardRepository;
        }

        public List<Card> GetUserCards(Guid userId)
        {
            return _cardRepo.GetCardsByUserId(userId);
        }

        public Card? GetCard(Guid cardId)
        {
            return _cardRepo.GetCardById(cardId);
        }

        public bool AddCard(Card card)
        {
            return _cardRepo.AddCard(card);
        }
    }
}
