using CurrencyExchange.Classes;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Services
{
    public class ExchangeRateService
    {
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly IMemoryCache _cache;
        private readonly FixerExchangeRateApiService _fixerApiService;

        private string GetKey(string exchangeCurrency, string baseCurrency) { return $"{baseCurrency}_{exchangeCurrency}"; }

        public ExchangeRateService(ILogger<ExchangeRateService> logger, IMemoryCache cache, FixerExchangeRateApiService fixerApiService)
        {
            _logger = logger;
            _cache = cache;
            _fixerApiService = fixerApiService;
        }

        public async Task<double?> GetLatestRate(string exchangeCurrency, string baseCurrency)
        {
            return (await GetLatest(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency))?.Rate;
        }

        public async Task<ExchangeRate?> GetLatest(string exchangeCurrency, string baseCurrency)
        {
            ExchangeRate? exchangeRate = null;

            var exchangeKey = GetKey(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency);

            var successfullGetFromCache = _cache.TryGetValue(exchangeKey, out exchangeRate);

            if (!successfullGetFromCache)
            {
                exchangeRate = await RefreshRate(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency);
            }

            return exchangeRate;
        }

        public async Task<ExchangeRate?> RefreshRate(string exchangeCurrency, string baseCurrency)
        {
            ExchangeRate? exchangeRate = null;

            var fixerExchangeRate = await _fixerApiService.GetLatest(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency);
            if (fixerExchangeRate != null)
            {
                exchangeRate = new ExchangeRate(x: fixerExchangeRate, exchangeCurrency: exchangeCurrency);
                var exchangeKey = GetKey(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency);
                _cache.Set(exchangeKey, exchangeRate, absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(30));
            }
            return exchangeRate;
        }

    }
}
