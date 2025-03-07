using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    public class DeckHandler
    {
        private readonly IDeckRepo _deckRepo;
        private readonly ICardRepo _cardRepo;

        public DeckHandler(IDeckRepo deckRepo, ICardRepo cardRepo)
        {
            _deckRepo = deckRepo;
            _cardRepo = cardRepo;
        }

        public List<Card> GetDeckByUserId(Guid userId)
        {
            return _deckRepo.GetDeckByUserId(userId);
        }

        public bool IsDeckValid(IEnumerable<Guid> cardIds, Guid userId)
        {
            // deck must contain exactly 4 cards
            if (cardIds.Count() != 4)
            {
                Console.WriteLine($"User {userId} owns no cards.");
                return false;
            }

            // cards must belong to user
            var userCards = _cardRepo.GetCardsByUserId(userId);
            return cardIds.All(id => userCards.Any(card => card.Id == id));
        }

        public bool UpdateDeck(Guid userId, List<Guid> cardIds)
        {
            if (!IsDeckValid(cardIds, userId))
            {
                throw new InvalidOperationException("Invalid deck config");
            }

            return _deckRepo.UpdateDeck(userId, cardIds);
        }
    }
}
