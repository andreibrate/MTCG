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
        Guid? GetAdminId();
        List<Card> GetCardsByIds(Guid[] cardIds);
        void DeletePackById(Guid packageId);
        List<Card>? GetAvailablePack();
        bool AddPack(Package package);
        int GetAvailablePackCount();
        bool TransferOwnership(List<Card> cards, Guid newOwnerId);
    }
}
