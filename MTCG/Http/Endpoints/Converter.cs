using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MTCG.Models;
using MTCG.Enums;

namespace MTCG.Http.Endpoints
{
    public class Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Card).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // read mandatory fields
            var name = jsonObject["Name"]?.Value<string>() ?? throw new JsonSerializationException("Card name is missing");
            var damage = jsonObject["Damage"]?.Value<double>() ?? throw new JsonSerializationException("Card damage is missing");

            // read or generate GUIDs
            Guid id = Guid.TryParse(jsonObject["Id"]?.Value<string>(), out var parsedId) ? parsedId : Guid.NewGuid();
            Guid ownerId = Guid.TryParse(jsonObject["OwnerId"]?.Value<string>(), out var parsedOwnerId) ? parsedOwnerId : Guid.NewGuid();

            // read or derive additional fields
            var cardType = jsonObject["CardType"]?.Value<int>() != null
                ? (CardType)jsonObject["CardType"]!.Value<int>()
                : GetCardTypeFromName(name);

            var tribe = cardType == CardType.Monster && jsonObject["Tribe"]?.Value<int>() != null
                ? (Tribe)jsonObject["Tribe"]!.Value<int>()
                : GetTribeFromName(name);

            var element = jsonObject["Element"]?.Value<int>() != null
                ? (Element)jsonObject["Element"]!.Value<int>()
                : GetElementFromName(name);

            // create card based on type
            Card card = cardType switch
            {
                CardType.Monster => new MonsterCard(name, damage, element, tribe, ownerId) { Id = id },
                CardType.Spell => new SpellCard(name, damage, element, ownerId) { Id = id },
                _ => throw new JsonSerializationException($"Unknown card type for name: {name}")
            };

            // fill additional JSON values
            serializer.Populate(jsonObject.CreateReader(), card);
            return card;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        private CardType GetCardTypeFromName(string name)
        {
            if (name.Contains("Spell", StringComparison.OrdinalIgnoreCase))
                return CardType.Spell;
            return CardType.Monster; // default
        }

        private Tribe GetTribeFromName(string name)
        {
            if (name.Contains("Dragon", StringComparison.OrdinalIgnoreCase))
                return Tribe.Dragon;
            if (name.Contains("Wizard", StringComparison.OrdinalIgnoreCase))
                return Tribe.Wizard;
            if (name.Contains("Ork", StringComparison.OrdinalIgnoreCase))
                return Tribe.Ork;
            if (name.Contains("Knight", StringComparison.OrdinalIgnoreCase))
                return Tribe.Knight;
            if (name.Contains("Kraken", StringComparison.OrdinalIgnoreCase))
                return Tribe.Kraken;
            if (name.Contains("Elf", StringComparison.OrdinalIgnoreCase))
                return Tribe.Elf;
            return Tribe.Goblin; // default
        }

        private Element GetElementFromName(string name)
        {
            if (name.Contains("Water", StringComparison.OrdinalIgnoreCase))
                return Element.Water;
            if (name.Contains("Fire", StringComparison.OrdinalIgnoreCase))
                return Element.Fire;
            return Element.Normal; // default
        }

    }
}
