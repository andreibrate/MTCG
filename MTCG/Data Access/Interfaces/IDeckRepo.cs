using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Data_Access.Interfaces
{
    public interface IDeckRepo
    {
        List<Card> GetDeckByUserId(Guid userId);
        bool UpdateDeck(Guid userId, IEnumerable<Guid> cardIds);
        bool RemoveCardsFromDeck(Guid userId, IEnumerable<Guid> cardIds);
    }
}
