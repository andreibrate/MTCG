using MTCG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class MonsterCard : Card
    {
        public double Health { get; set; }
        public Tribe Tribe { get; }

        public MonsterCard(string name, double damage, double health, Element elementType, CardType cardType, Tribe tribe, Guid? ownerId) 
            : base(name, damage, elementType, cardType, ownerId ?? Guid.NewGuid())
        {
            this.Health = health;
            this.Tribe = tribe;
        }
    }
}
