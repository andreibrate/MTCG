using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Enums;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    public class BattleSpecialConditions
    {
        public int DecideWinner(Card card1, Card card2)
        {
            if (card1 is MonsterCard monster1 && card2 is MonsterCard monster2)
            {
                return CompareMonsters(monster1, monster2);
            }
            else if (card1 is SpellCard spell1 && card2 is SpellCard spell2)
            {
                return CompareSpells(spell1, spell2);
            }
            else if (card1 is MonsterCard monsterCard && card2 is SpellCard spellCard)
            {
                return CompareMonsterWithSpell(monsterCard, spellCard);
            }
            else if (card1 is SpellCard spellCard1 && card2 is MonsterCard monsterCard1)
            {
                return -CompareMonsterWithSpell(monsterCard1, spellCard1);
            }

            // default: compare damage
            return card1.Damage.CompareTo(card2.Damage);
        }

        private int CompareMonsters(MonsterCard monster1, MonsterCard monster2)
        {
            // Dragon vs. Goblin = Dragon wins
            if (monster1.Tribe == Tribe.Dragon && monster2.Tribe == Tribe.Goblin)
                return 1;
            if (monster2.Tribe == Tribe.Dragon && monster1.Tribe == Tribe.Goblin)
                return -1;

            // default: compare damage
            return monster1.Damage.CompareTo(monster2.Damage);
        }

        private int CompareSpells(SpellCard spell1, SpellCard spell2)
        {
            // Water vs. Fire = Water wins
            if (spell1.Element == Element.Water && spell2.Element == Element.Fire)
                return 1;
            if (spell2.Element == Element.Water && spell1.Element == Element.Fire)
                return -1;

            // default: compare damage
            return spell1.Damage.CompareTo(spell2.Damage);
        }

        private int CompareMonsterWithSpell(MonsterCard monster, SpellCard spell)
        {
            // lose against weakness
            if (monster.Element == Element.Normal && spell.Element == Element.Fire) // normal monster vs. fire spell = lose
                return -1;
            if (monster.Element == Element.Fire && spell.Element == Element.Water)  // fire monster vs. water spell = lose
                return -1;
            if (monster.Element == Element.Water && spell.Element == Element.Normal) // water monster vs. normal spell = lose
                return -1;

            // win against strength
            if (monster.Element == Element.Normal && spell.Element == Element.Water) // normal monster vs. water spell = win
                return 1;
            if (monster.Element == Element.Fire && spell.Element == Element.Normal)  // fire monster vs. normal spell = win
                return 1;
            if (monster.Element == Element.Water && spell.Element == Element.Fire)   // water monster vs. fire spell = win
                return 1;

            // default: compare damage
            return monster.Damage.CompareTo(spell.Damage);
        }
    }
}
