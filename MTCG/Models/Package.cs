using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class Package
    {
        public List<Card> Cards { get; set; }
        public Package(List<Card> cards)
        {
            Cards = cards;
        }
    }
}
