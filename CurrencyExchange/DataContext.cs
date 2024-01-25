using CurrencyExchange.Classes;
using CurrencyExchange.DataModels;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
        : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Trade
            modelBuilder.Entity<Trade>()
                .HasIndex(x => x.ClientId).IsUnique(false);
            modelBuilder.Entity<Trade>()
                .HasIndex(x => x.TimestampUTC).IsUnique(false);
            #endregion
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Trade> Trades { get; set; }
    }
}
