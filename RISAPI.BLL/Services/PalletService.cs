using Microsoft.EntityFrameworkCore;
using RISAPI.Client.Response;
using RISAPI.Database;
using RISAPI.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RISAPI.BLL.Services
{
    public class PalletService
    {
        private readonly RISAPIContext _context;

        public PalletService(RISAPIContext context)
        {
            _context = context;
        }

        public GetPalletResponse GetPallet(int palletId)
        {
            var pallet = _context.Pallets
                .Include(x => x.Size)
                .SingleOrDefault(x => x.Id == palletId);

            if (pallet == null)
            {
                throw new ArgumentException($"Pallet: {palletId} does not exists");
            }

            TakeDownPallet(pallet);

            return new GetPalletResponse
            {
                Size = new SizeDto
                {
                    Length = pallet.Size.Length,
                    Width = pallet.Size.Width,
                    Height = pallet.Size.Height
                },
                Weight = pallet.Weight
            };
        }

        private void TakeDownPallet(Pallet pallet)
        {
            pallet.IsPlaced = false;
            _context.Update(pallet);
            _context.SaveChanges();

        }

    }
}