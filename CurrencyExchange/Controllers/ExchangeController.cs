using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {
        private readonly ILogger<ExchangeController> _logger;
        private readonly DataContext _dataContext;
        private readonly int ExchangeRateValidityPeriod = 30;
        public ExchangeController(ILogger<ExchangeController> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [HttpGet(Name = "GetLatestExchangeRate")]
        public ActionResult GetLatestExchangeRate(string baseCurrency, string exchangeCurrency)
        {
            try
            {
                var exchangeRate = _dataContext.ExchangeRates.FirstOrDefault(x =>
                x.BaseCurrency == baseCurrency &&
                x.ExchangeCurrency == exchangeCurrency &&
                x.LastUpdatedUTC >= DateTime.UtcNow.AddMinutes(-1 * ExchangeRateValidityPeriod));

                if (exchangeRate == null) { return BadRequest("No data was found."); }

                return Ok(exchangeRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
