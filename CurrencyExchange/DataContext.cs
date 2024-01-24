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
            #region ExchangeRate
            modelBuilder.Entity<ExchangeRate>()
                .HasIndex(x => x.BaseCurrency).IsUnique(false);
            modelBuilder.Entity<ExchangeRate>()
                .HasIndex(x => x.ExchangeCurrency).IsUnique(false);
            modelBuilder.Entity<ExchangeRate>()
                .HasIndex(x => x.LastUpdatedUTC).IsUnique(false);
            #endregion

            #region Trade
            modelBuilder.Entity<Trade>()
                .HasIndex(x => x.ClientId).IsUnique(false);
            modelBuilder.Entity<Trade>()
                .HasIndex(x => x.ExchangeRateId).IsUnique(false);
            modelBuilder.Entity<Trade>()
                .HasIndex(x => x.TimestampUTC).IsUnique(false);
            #endregion
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<Trade> Trades { get; set; }
    }
}
