using CurrencyExchange.Classes;
using CurrencyExchange.DataModels;
using CurrencyExchange.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ILogger<TradeController> _logger;
        private readonly DataContext _dataContext;
        private readonly FixerExchangeRateApiService _fixerExchangeRateApiService;
        private readonly ExchangeRateService _exchangeRateService;
        private readonly int ClientTradeLimitValidtityPeriod = 60;
        private readonly int ClientTradeLimitCount = 10;

        public TradeController(ILogger<TradeController> logger, DataContext dataContext, FixerExchangeRateApiService fixerExchangeRateApiService, ExchangeRateService exchangeRateService)
        {
            _logger = logger;
            _dataContext = dataContext;
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

                    var countOfTradesInTimeLimitValidityPeriod = _dataContext.Trades.Count(x =>
                        x.ClientId == tradeOrder.ClientId &&
                        x.TimestampUTC >= DateTime.UtcNow.AddMinutes(-1 * ClientTradeLimitValidtityPeriod)
                    );

                    if (countOfTradesInTimeLimitValidityPeriod > ClientTradeLimitCount) { return BadRequest("Trade limit treshhold has been exceeded."); }

                    var exchangeRate = await _exchangeRateService.GetLatest(exchangeCurrency: tradeOrder.ExchangeCurrency, baseCurrency: tradeOrder.BaseCurrency);
                    if (exchangeRate == null) { return BadRequest("Exchange rates are unavailable at this time."); }

                    var trade = new Trade(exchangeRate: exchangeRate, client: client, amount: tradeOrder.Amount);
                    _dataContext.Trades.Add(trade);
                    _dataContext.SaveChanges();
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
