using CurrencyExchange.Classes;
using CurrencyExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class FixerExchangeRateApiServiceTests
    {
        private readonly Mock<ILogger<FixerExchangeRateApiService>> _mockLogger;
        private readonly Mock<IRestClient> _mockRestClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private Helper _helper { get; set; } = new Helper();

        public FixerExchangeRateApiServiceTests()
        {
            _mockLogger = new Mock<ILogger<FixerExchangeRateApiService>>();
            _mockRestClient = new Mock<IRestClient>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        [Fact]
        public void CheckExchangeRateSavedToCacheOnSuccess()
        {
            //initialize cache
            var cache = _helper.CreateMemoryCache();

            //Arrange test data
            var date = new DateTime(2024, 01, 26, 21, 43, 23);
            var dateTimeOffset = new DateTimeOffset(date).ToUniversalTime();
            var ticks = date.Ticks;
            var baseCurrency = "EUR";
            var exchangeCurrency = "GBP";
            var rates = new Dictionary<string, double> { { exchangeCurrency, 34.34 } };

            //Arrange test response
            var fixerExchangeRateResponse = new FixerExchangeRate
            {
                Success = true,
                Timestamp = ticks,
                Base = baseCurrency,
                Date = dateTimeOffset,
                Rates = rates
            };
            var mockRestResponse = new Mock<RestResponse>();
            mockRestResponse.Setup(x => x.Content).Returns(JsonConvert.SerializeObject(fixerExchangeRateResponse));
            mockRestResponse.Setup(x => x.IsSuccessStatusCode).Returns(true);

            //Initialize Service
            var service = new FixerExchangeRateApiService(_mockLogger.Object, _mockConfiguration.Object, cache);

            // Act
            var result = service.GetLatest(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

    }
}
