using MTCG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public abstract class Card
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public Element ElementType { get; set; }
        public CardType CardType { get; set; }
        public Guid OwnerId { get; set; }
        public bool isLocked { get; set; }


        protected Card(string name, double damage, Element elementType, CardType cardType, Guid ownerId)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Damage = damage;
            this.ElementType = elementType;
            this.CardType = cardType;
            this.OwnerId = ownerId;
            this.isLocked = false;
        }

    }
}
