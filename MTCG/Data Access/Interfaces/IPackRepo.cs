using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Data_Access.Interfaces
{
    public interface IPackRepo
    {
        List<Card> GetPackByUserId(Guid userId);
        bool AddCardToPack(Guid userId, IEnumerable<Guid> cardIds);
        bool RemoveCardFromPack(Guid userId, IEnumerable<Guid> cardIds);
    }
}
