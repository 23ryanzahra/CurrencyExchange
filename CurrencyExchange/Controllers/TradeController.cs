using CurrencyExchange.Classes;
using CurrencyExchange.DataModels;
using CurrencyExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ILogger<TradeController> _logger;
        private readonly DataContext _dataContext;
        private readonly IMemoryCache _cache;
        private readonly FixerExchangeRateApiService _fixerExchangeRateApiService;
        private readonly ExchangeRateService _exchangeRateService;
        private readonly int ClientTradeLimitCount = 10;

        public TradeController(ILogger<TradeController> logger, DataContext dataContext, IMemoryCache cache, FixerExchangeRateApiService fixerExchangeRateApiService, ExchangeRateService exchangeRateService)
        {
            _logger = logger;
            _dataContext = dataContext;
            _cache = cache;
            _fixerExchangeRateApiService = fixerExchangeRateApiService;
            _exchangeRateService = exchangeRateService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<TradeDTO?>> AddNew(TradeOrder tradeOrder)
        {
            using (var transaction = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    if (tradeOrder.BaseCurrency != "EUR") { return StatusCode(105, "Current third party API subscription only supports EUR as the base currency."); }

                    if (!await _fixerExchangeRateApiService.IsValidSymbol(tradeOrder.ExchangeCurrency)) { return BadRequest("Invalid exchange currency."); }
                    if (!await _fixerExchangeRateApiService.IsValidSymbol(tradeOrder.BaseCurrency)) { return BadRequest("Invalid base currency."); }

                    var client = await _dataContext.Clients.FindAsync(tradeOrder.ClientId);
                    if (client == null) { return BadRequest("Client not found."); }

                    var timestamps = _cache.Get<List<DateTime>>(tradeOrder.ClientId) ?? new List<DateTime>();
                    timestamps.RemoveAll(x => x < DateTime.UtcNow.AddHours(-1));

                    if (timestamps.Count >= ClientTradeLimitCount) { return StatusCode(429, "Rate limit exceeded. You can only make 10 trades per hour."); }

                    var exchangeRate = await _exchangeRateService.GetLatest(exchangeCurrency: tradeOrder.ExchangeCurrency, baseCurrency: tradeOrder.BaseCurrency);
                    if (exchangeRate == null) { return BadRequest("Exchange rates are unavailable at this time."); }

                    var trade = new Trade(exchangeRate: exchangeRate, client: client, amount: tradeOrder.Amount);
                    _dataContext.Trades.Add(trade);
                    _dataContext.SaveChanges();

                    timestamps.Add(DateTime.UtcNow);

                    _cache.Set(tradeOrder.ClientId, timestamps, TimeSpan.FromHours(1));

                    transaction.Commit();
                    return Ok(new TradeDTO(trade));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "An error occurred while processing your request.");
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }
        }
    }
}
