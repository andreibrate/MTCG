using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Data_Access.Interfaces
{
    public interface ICardRepo
    {
        List<Card> GetCardsByUserId(Guid userId);
        Card? GetCardById(Guid cardId);
        bool AddCard(Card card);
        void UpdateCard(Card card);
    }
}
