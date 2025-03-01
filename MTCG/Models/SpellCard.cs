using MTCG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string name, double damage, Element elementType, Guid? ownerId) 
            : base(name, damage, elementType, CardType.Spell, ownerId ?? Guid.NewGuid())
        {
        }
    }
}
