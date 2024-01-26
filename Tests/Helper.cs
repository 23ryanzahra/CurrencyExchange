using CurrencyExchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    public class Helper
    {
        public IMemoryCache CreateMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IMemoryCache>();
        }

        public DataContext CreateDataContext(string databaseName)
        {
            //Setup in memory database
            var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            return new DataContext(options);
        }
    }
}
