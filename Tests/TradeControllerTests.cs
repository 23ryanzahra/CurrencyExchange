using CurrencyExchange.Classes;
using CurrencyExchange.Services;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public class TradeControllerTests
    {
        private Helper _helper { get; set; } = new Helper();

        private readonly Mock<ILogger<TradeController>> _mockLogger;
        private Mock<FixerExchangeRateApiService> _mockFixerExchangeRateApiService;
        private Mock<ExchangeRateService> _mockExchangeRateService;
        private IMemoryCache _cache;
        private readonly string _symbolsKey = "symbols";
        private readonly Mock<ILogger<FixerExchangeRateApiService>> _mockFixerExchangeRateApiServiceLogger = new Mock<ILogger<FixerExchangeRateApiService>>();
        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<ILogger<ExchangeRateService>> _mockExchangeRateServiceLogger = new Mock<ILogger<ExchangeRateService>>();

        private readonly string _baseCurrency = "EUR";
        private readonly string _exchangeCurrency = "GBP";
        private readonly string _baseCurrencyName = "Euro";
        private readonly string _exchangeCurrencyName = "British Pound Sterling";
        private Dictionary<string, string> _validCurrencies;
        private Dictionary<string, string> _invalidCurrencies = new Dictionary<string, string> { { "INVALID", "InvalidCurrency" } };


        public TradeControllerTests()
        {
            _mockLogger = new Mock<ILogger<TradeController>>();

            _cache = _helper.CreateMemoryCache();
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            _validCurrencies = new Dictionary<string, string> { { _baseCurrency, _baseCurrencyName }, { _exchangeCurrency, _exchangeCurrencyName } };
        }

        [Fact]
        public async Task AddNew_ReturnsBadRequest_WhenBaseCurrencyIsNotEUR()
        {
            //initialize datacontext
            using var context = _helper.CreateDataContext(databaseName: "AddNew_ReturnsBadRequest_WhenBaseCurrencyIsNotEUR");

            // Arrange test data
            var tradeOrder = new TradeOrder { BaseCurrency = "USD" };
            var controller = new TradeController(_mockLogger.Object, context, _cache, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.AddNew(tradeOrder);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(105, objectResult.StatusCode);
        }

        [Fact]
        public async Task AddNew_ReturnsBadRequest_WhenCurrenciesNotValid()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _invalidCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            //reinitialize _mockFixerExchangeRateApiService  to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);

            //initilize datacontext
            using var context = _helper.CreateDataContext(databaseName: "AddNew_ReturnsBadRequest_WhenBaseCurrencyIsNotEUR");

            // Arrange test data
            var tradeOrder = new TradeOrder { BaseCurrency = _baseCurrency, ExchangeCurrency = "YEN" };
            var controller = new TradeController(_mockLogger.Object, context, _cache, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.AddNew(tradeOrder);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid exchange currency.", objectResult.Value);
        }

        [Fact]
        public async Task AddNew_ReturnsBadRequest_WhenClientNotFound()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            //reinitialize _mockFixerExchangeRateApiService  to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);

            //initilize datacontext
            using var context = _helper.CreateDataContext(databaseName: "AddNew_ReturnsBadRequest_WhenClientNotFound");

            // Arrange test data
            var tradeOrder = new TradeOrder { BaseCurrency = _baseCurrency, ExchangeCurrency = _exchangeCurrency, ClientId = 1, Amount = 10 };
            var controller = new TradeController(_mockLogger.Object, context, _cache, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.AddNew(tradeOrder);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Client not found.", objectResult.Value);
        }

        [Fact]
        public async Task AddNew_ReturnsBadRequest_WhenExceedingRateLimit()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));
            _cache.Set(1, Enumerable.Range(0, 12).Select(i => DateTime.Now.AddMinutes(-i)).ToList(), TimeSpan.FromHours(1));

            //reinitialize _mockFixerExchangeRateApiService to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            //initilize datacontext
            using var context = _helper.CreateDataContext(databaseName: "AddNew_ReturnsBadRequest_WhenExceedingRateLimit");

            // Arrange tst data
            context.Clients.Add(new Client("Joe", "Doe"));
            context.SaveChanges();

            var tradeOrder = new TradeOrder { BaseCurrency = _baseCurrency, ExchangeCurrency = _exchangeCurrency, ClientId = 1, Amount = 10 };
            var controller = new TradeController(_mockLogger.Object, context, _cache, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.AddNew(tradeOrder);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(429, objectResult.StatusCode);
            Assert.Equal("Rate limit exceeded. You can only make 10 trades per hour.", objectResult.Value);
        }

        [Fact]
        public async Task AddNew_OK_Succeeded()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));
            _cache.Set(1, Enumerable.Range(0, 5).Select(i => DateTime.Now.AddMinutes(-i)).ToList(), TimeSpan.FromHours(1));
            _cache.Set("EUR_GBP", new ExchangeRate { BaseCurrency = _baseCurrency, ExchangeCurrency = _exchangeCurrency, Rate = 1.234, LastUpdatedUTC = DateTime.UtcNow.AddMinutes(-5) });

            //reinitialize _mockFixerExchangeRateApiService and _mockExchangeRateService to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            //initilize datacontext
            using var context = _helper.CreateDataContext(databaseName: "AddNew_ReturnsBadRequest_WhenExceedingRateLimit");

            // Arrange test data
            context.Clients.Add(new Client("Joe", "Doe"));
            context.SaveChanges();

            var tradeOrder = new TradeOrder { BaseCurrency = _baseCurrency, ExchangeCurrency = _exchangeCurrency, ClientId = 1, Amount = 10 };
            var controller = new TradeController(_mockLogger.Object, context, _cache, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.AddNew(tradeOrder);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var trade = Assert.IsType<TradeDTO>(okResult.Value);
            Assert.Equal(_baseCurrency, trade.BaseCurrency);
            Assert.Equal(10, trade.BaseCurrencyAmount);
            Assert.Equal(_exchangeCurrency, trade.ExchangedCurrency);
            Assert.Equal(12.34, trade.ExchangedCurrencyAmount);
            Assert.Equal(1.234, trade.CurrentRate);
        }
    }
}
