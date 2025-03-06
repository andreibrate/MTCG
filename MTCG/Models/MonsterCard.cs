﻿using MTCG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class MonsterCard : Card
    {
        public Tribe Tribe { get; }

        public MonsterCard(string name, double damage, Element elementType, Tribe tribe, Guid? ownerId) 
            : base(name, damage, elementType, CardType.Monster, ownerId ?? Guid.NewGuid())
        {
            this.Tribe = tribe;
        }
    }
}
