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
        void DeletePackageById(Guid packageId);
        List<Card>? GetAvailablePackage();
        bool AddPackage(Package package);
        int GetAvailablePackageCount();
        bool TransferOwnership(List<Card> cards, Guid newOwnerId);
    }
}
