using CurrencyExchange.Classes;
using CurrencyExchange.Services;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public class ExchangeRateControllerTests
    {
        private Helper _helper { get; set; } = new Helper();

        private readonly Mock<ILogger<ExchangeRateController>> _mockLogger;
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

        public ExchangeRateControllerTests()
        {
            _mockLogger = new Mock<ILogger<ExchangeRateController>>();

            _cache = _helper.CreateMemoryCache();
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            _validCurrencies = new Dictionary<string, string> { { _baseCurrency, _baseCurrencyName }, { _exchangeCurrency, _exchangeCurrencyName } };
        }

        [Fact]
        public async Task GetLatest_ReturnsBadRequest_WhenBaseCurrencyIsNotEUR()
        {
            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.GetLatest(exchangeCurrency: _exchangeCurrency, baseCurrency: "INVALID");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(105, objectResult.StatusCode);
            Assert.Equal("Current third party API subscription only supports EUR as the base currency.", objectResult.Value);
        }

        [Fact]
        public async Task GetLatest_ReturnsBadRequest_WhenCurrenciesNotValid()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _invalidCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            //reinitialize _mockFixerExchangeRateApiService  to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.GetLatest(exchangeCurrency: _exchangeCurrency, baseCurrency: _baseCurrency);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid exchange currency.", objectResult.Value);
        }

        [Fact]
        public async Task GetLatest_ReturnsInternalError_DueToToThirdPartyAPINotWorkingAndRateExpired()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));
            _cache.Set("EUR_GBP", new ExchangeRate { BaseCurrency = _baseCurrency, ExchangeCurrency = _exchangeCurrency, Rate = 1.234, LastUpdatedUTC = DateTime.UtcNow.AddMinutes(-55) }, absoluteExpiration: DateTime.UtcNow.AddMinutes(-55));

            //reinitialize _mockFixerExchangeRateApiService and _mockExchangeRateService to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.GetLatest(exchangeCurrency: _exchangeCurrency, baseCurrency: _baseCurrency);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("An error occurred while processing your request.", objectResult.Value);
        }

        [Fact]
        public async Task GetLatest_OK_Succeeded()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));
            _cache.Set("EUR_GBP", new ExchangeRate { BaseCurrency = _baseCurrency, ExchangeCurrency = _exchangeCurrency, Rate = 1.234, LastUpdatedUTC = DateTime.UtcNow.AddMinutes(-5) });

            //reinitialize _mockFixerExchangeRateApiService and _mockExchangeRateService to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.GetLatest(exchangeCurrency: _exchangeCurrency, baseCurrency: _baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var exchangeRate = Assert.IsType<ExchangeRate>(okResult.Value);
            Assert.Equal(_baseCurrency, exchangeRate.BaseCurrency);
            Assert.Equal(_exchangeCurrency, exchangeRate.ExchangeCurrency);
            Assert.True(exchangeRate.LastUpdatedUTC >= DateTime.UtcNow.AddMinutes(-30));
            Assert.Equal(1.234, exchangeRate.Rate);
        }

        [Fact]
        public async Task IsValidCurrency_OK_WithTrueValue_WhenValidCurrency()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            //reinitialize _mockFixerExchangeRateApiService and _mockExchangeRateService to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.IsValidCurrency(currency: _baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var isValid = Assert.IsType<bool>(okResult.Value);
            Assert.True(isValid);
        }

        [Fact]
        public async Task IsValidCurrency_OK_WithFalseValue_WhenInvalidCurrency()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _invalidCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            //reinitialize _mockFixerExchangeRateApiService and _mockExchangeRateService to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);
            _mockExchangeRateService = new Mock<ExchangeRateService>(_mockExchangeRateServiceLogger.Object, _cache, _mockFixerExchangeRateApiService.Object);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.IsValidCurrency(currency: _baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var isValid = Assert.IsType<bool>(okResult.Value);
            Assert.False(isValid);
        }

        [Fact]
        public async Task GetValidCurrencies_ReturnsInternalError_WhenCurrenciesNotAvailable()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();

            //reinitialize _mockFixerExchangeRateApiService  to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.GetValidCurrencies();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal("An error occurred while processing your request.", objectResult.Value);
        }

        [Fact]
        public async Task GetValidCurrencies_Ok_WhenCurrenciesAreAvailable()
        {
            //reinitialize cache and setting new values
            _cache = _helper.CreateMemoryCache();
            _cache.Set(_symbolsKey, _validCurrencies, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            //reinitialize _mockFixerExchangeRateApiService  to set new cache values
            _mockFixerExchangeRateApiService = new Mock<FixerExchangeRateApiService>(_mockFixerExchangeRateApiServiceLogger.Object, _mockConfiguration.Object, _cache);

            // Arrange test data
            var controller = new ExchangeRateController(_mockLogger.Object, _mockFixerExchangeRateApiService.Object, _mockExchangeRateService.Object);

            // Act
            var result = await controller.GetValidCurrencies();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var validCurrencies = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.Equal(_validCurrencies, validCurrencies);
        }
    }
}
