using Microsoft.EntityFrameworkCore;
using RISAPI.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RISAPI.Database
{
    public class RISAPIContext : DbContext
    {
        public virtual DbSet<Pallet> Pallets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }
            optionsBuilder.UseSqlServer($"Server=tr1qud9eg9qu8gw.c1bh0avymwpf.eu-west-1.rds.amazonaws.com,1433; Database=TDFRPOCRISDB; User Id= admin; Password=Test1234!");
            base.OnConfiguring(optionsBuilder);
        }



    }
}