using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Enums;

namespace MTCG.Models
{
    public class Trading
    {
        // ctor for new trade
        public Trading(Guid? id, Card? tradedCard, Element wantedElement, Tribe wantedTribe, float wantedMinDamage)
        {
            Id = id ?? Guid.NewGuid();
            TradedCard = tradedCard;
            TradedCardId = tradedCard?.Id;
            WantedElement = wantedElement;
            WantedTribe = wantedTribe;
            WantedMinDamage = wantedMinDamage;
        }


        public Guid Id { get; set; }                // id of trade deal
        public Card? TradedCard { get; set; }       // offered card
        public Guid? TradedCardId { get; set; }     // id of offered card
        public Element WantedElement { get; set; }  // requirement for specific element of the card
        public Tribe WantedTribe { get; set; }      // requirement for specific tribe of the card
        public float WantedMinDamage { get; set; }  // requirement for minimum damage of the card 
    }
}
