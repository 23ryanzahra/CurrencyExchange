using CurrencyExchange.Classes;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RestSharp;

namespace CurrencyExchange.Services
{
    public class FixerExchangeRateApiService
    {
        private readonly ILogger<FixerExchangeRateApiService> _logger;
        private readonly IMemoryCache _cache;
        private readonly Uri _baseUrl;
        private readonly string _accessKey;
        private readonly string _symbolsKey = "symbols";
        public FixerExchangeRateApiService(ILogger<FixerExchangeRateApiService> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            _baseUrl = new Uri(configuration["FixerApi:BaseUrl"] ?? "");
            _accessKey = configuration["FixerApi:AccessKey"] ?? "";
        }

        public async Task<FixerExchangeRate?> GetLatest(string exchangeCurrency, string baseCurrency = "EUR")
        {
            FixerExchangeRate? fixerExchangeRate = null;

            var options = new RestClientOptions()
            {
                MaxTimeout = -1,
                BaseUrl = _baseUrl
            };
            var client = new RestClient(options);
            var request = new RestRequest($"latest?access_key={_accessKey}&symbols={exchangeCurrency}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(response.Content))
            {
                fixerExchangeRate = JsonConvert.DeserializeObject<FixerExchangeRate>(response.Content);
            }
            else
            {
                throw new Exception(JsonConvert.SerializeObject(response));
            }
            _logger.LogInformation($"FixerAPI-GetExchangeRates:{baseCurrency}", response.Content);


            return fixerExchangeRate;
        }

        public async Task<Dictionary<string, string>?> RefreshSymbols()
        {
            FixerSymbols? fixerSymbols = null;

            var options = new RestClientOptions()
            {
                MaxTimeout = -1,
                BaseUrl = _baseUrl
            };
            var client = new RestClient(options);
            var request = new RestRequest($"symbols?access_key={_accessKey}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(response.Content))
            {
                fixerSymbols = JsonConvert.DeserializeObject<FixerSymbols>(response.Content);
            }
            else
            {
                throw new Exception(JsonConvert.SerializeObject(response));
            }

            _cache.Set(_symbolsKey, fixerSymbols?.Symbols, absoluteExpirationRelativeToNow: TimeSpan.FromDays(7));

            _logger.LogInformation($"FixerAPI-GetSymbols: {response.Content}", response.Content);
            return fixerSymbols?.Symbols;
        }

        public async Task<bool> IsValidSymbol(string symbol)
        {
            var symbols = await GetSymbolsDictionary();

            return symbols != null && symbols.ContainsKey(symbol);
        }

        public async Task<Dictionary<string, string>?> GetSymbolsDictionary()
        {
            Dictionary<string, string>? dictionary = null;

            var successfullGetFromCache = _cache.TryGetValue(_symbolsKey, out dictionary);

            if (!successfullGetFromCache || dictionary == null)
            {
                dictionary = await RefreshSymbols();
            }

            return dictionary;
        }
    }

}
