using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    internal class Deck
    {
        public List<Card> Cards { get; set; } = new List<Card>();

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            Cards.Remove(card);
        }

        // draw random card from deck
        public Card DrawCard()
        {
            if (Cards.Count == 0)
            {
                throw new InvalidOperationException("Deck is empty");
            }
            Random random = new Random();
            int index = random.Next(Cards.Count); // random index between 0 and cards.count
            Card CardDrawn = Cards[index];
            Cards.RemoveAt(index);
            return CardDrawn;
        }
    }
}

