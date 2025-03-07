using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    public class ScoreboardHandler
    {
        private readonly IUserRepo _userRepo;

        public ScoreboardHandler(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public IEnumerable<(string Username, Stats Stats)> GetScoreboard()
        {
            return _userRepo.GetScoreboardStats();
        }
    }
}
