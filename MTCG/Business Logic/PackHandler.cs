using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Data_Access.Interfaces;
using MTCG.Models;

namespace MTCG.Business_Logic
{
    internal class PackHandler
    {
        private readonly IPackRepo _packRepo;

        public PackHandler(IPackRepo packRepo)
        {
            _packRepo = packRepo;
        }

        public bool CreatePack(List<Card> cards)
        {
            if (cards.Count != 5)
            {
                return false;
            }

            var adminId = _packRepo.GetAdminId();

            if (!adminId.HasValue)
            {
                Console.WriteLine("Admin ID does not exist."); // => cancel
                return false;
            }

            foreach (var card in cards)
            {
                card.OwnerId = adminId.Value; // set admin as owner
            }

            var package = new Package(cards);
            return _packRepo.AddPackage(package);
        }
    }
}
