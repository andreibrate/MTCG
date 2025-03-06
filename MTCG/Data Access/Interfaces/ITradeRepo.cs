using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Data_Access.Interfaces
{
    public interface ITradeRepo
    {
        List<Trading> GetTrades();
        Trading? GetTradeById(Guid tradeId);
        void AddTrade(Trading trade);
        void DeleteTrade(Guid tradeId);
    }
}
