using CurrencyExchange.DataModels;

namespace CurrencyExchange.Classes
{
    public class TradeDTO
    {
        public TradeDTO()
        {

        }

        public TradeDTO(Trade x)
        {
            TimestampUTC = x.TimestampUTC;
            BaseCurrency = x.BaseCurrency;
            BaseCurrencyAmount = x.BaseCurrencyAmount;
            ExchangedCurrency = x.ExchangedCurrency;
            ExchangedCurrencyAmount = x.ExchangedCurrencyAmount;
            CurrentRate = x.CurrentRate;
        }
        public DateTime TimestampUTC { get; set; }

        public string BaseCurrency { get; set; }

        public double BaseCurrencyAmount { get; set; }

        public string ExchangedCurrency { get; set; }

        public double ExchangedCurrencyAmount { get; set; }

        public double CurrentRate { get; set; }
    }
}
