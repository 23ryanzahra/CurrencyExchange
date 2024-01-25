using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyExchange.Classes
{
    public class ExchangeRate
    {
        public ExchangeRate()
        {

        }

        public ExchangeRate(FixerExchangeRate x, string exchangeCurrency)
        {
            BaseCurrency = x.Base;
            ExchangeCurrency = exchangeCurrency;
            Rate = x.Rates[exchangeCurrency];
            LastUpdatedUTC = DateTime.UtcNow;
        }

        public string BaseCurrency { get; set; }

        public string ExchangeCurrency { get; set; }

        public double Rate { get; set; }

        public DateTime LastUpdatedUTC { get; set; }
    }
}
