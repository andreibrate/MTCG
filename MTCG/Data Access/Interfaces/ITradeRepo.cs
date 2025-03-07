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
        void AddTrade(Trading trade);
        Trading? GetTradeById(Guid tradeId);
        void DeleteTrade(Guid tradeId);
        List<Trading> GetTrades();
    }
}
