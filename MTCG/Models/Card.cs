﻿using MTCG.Enums;
using Newtonsoft.Json;
using MTCG.Http.Endpoints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    [JsonConverter(typeof(Converter))]
    public abstract class Card
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public Element Element { get; set; }
        public CardType CardType { get; set; }
        public Guid OwnerId { get; set; }
        public bool IsLocked { get; set; }


        protected Card(string name, double damage, Element element, CardType cardType, Guid ownerId)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Damage = damage;
            this.Element = element;
            this.CardType = cardType;
            this.OwnerId = ownerId;
            this.IsLocked = false;
        }

    }
}
