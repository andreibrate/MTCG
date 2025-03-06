using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Data_Access.Interfaces
{
    public interface IUserRepo
    {
        bool RegisterUser(string username, string password);
        User? LoginUser(string username, string password);
        User? GetUserToken(string token);
        void UpdateUser(User user);
        List<User> GetUsers();
        List<User> GetUsersStartingWith(string prefix);
        IEnumerable<(string Username, Stats Stats)> GetScoreboardStats();
    }
}
