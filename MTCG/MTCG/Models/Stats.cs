using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class Stats
    {
        // constructor, start values
        public Stats() {
            Elo = 100;
            Wins = 0;
            Losses = 0;
        }

        public int Elo { get; set; } // Elo rating for leaderboard
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}
