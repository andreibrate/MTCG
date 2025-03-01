using MTCG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    internal class MonsterCard : Card
    {
        public double Health { get; set; }
        public MonsterCard(string name, double damage, double health, Element elementType, CardType cardType) : base(name, damage, elementType, cardType)
        {
            this.Health = health;
        }
    }
}
