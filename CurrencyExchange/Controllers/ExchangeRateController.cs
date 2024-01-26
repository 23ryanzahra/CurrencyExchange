using CurrencyExchange.Classes;
using CurrencyExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ILogger<ExchangeRateController> _logger;
        private readonly FixerExchangeRateApiService _fixerExchangeRateApiService;
        private readonly ExchangeRateService _exchangeRateService;
        public ExchangeRateController(ILogger<ExchangeRateController> logger, FixerExchangeRateApiService fixerExchangeRateApiService, ExchangeRateService exchangeRateService)
        {
            _logger = logger;
            _fixerExchangeRateApiService = fixerExchangeRateApiService;
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<ExchangeRate>> GetLatest([Required]string exchangeCurrency, [Required]string baseCurrency = "EUR")
        {
            try
            {
                //Fixer AI basic subscription plan only supports EUR as the base currency, but all logic in the project supports multiple base currencies
                if (baseCurrency != "EUR") { return StatusCode(105, "Current third party API subscription only supports EUR as the base currency."); }

                if (!await _fixerExchangeRateApiService.IsValidSymbol(exchangeCurrency)) { return BadRequest("Invalid exchange currency."); }
                if (!await _fixerExchangeRateApiService.IsValidSymbol(baseCurrency)) { return BadRequest("Invalid base currency."); }

                ExchangeRate? exchangeRate = await _exchangeRateService.GetLatest(exchangeCurrency: exchangeCurrency, baseCurrency: baseCurrency);
                if (exchangeRate == null) { return BadRequest("No data was found."); }
                return Ok(exchangeRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<bool>> IsValidCurrency([Required]string currency)
        {
            try
            {
                return Ok(await _fixerExchangeRateApiService.IsValidSymbol(symbol: currency));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<Dictionary<string, string>?>> GetValidCurrencies()
        {
            try
            {
                var symbols = await _fixerExchangeRateApiService.GetSymbolsDictionary();
                if (symbols == null) { return BadRequest("No data was found."); }
                return Ok(symbols);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}

