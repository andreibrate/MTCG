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
        public Tribe Tribe { get; set; }

        public MonsterCard(string name, double damage, Element element, Tribe tribe, Guid? ownerId) 
            : base(name, damage, element, CardType.Monster, ownerId ?? Guid.NewGuid())
        {
            this.Tribe = tribe;
        }
    }
}
