using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CurrencyExchange.Classes;

namespace CurrencyExchange.DataModels
{
    public class Trade
    {
        public Trade()
        {
                
        }

        public Trade(ExchangeRate exchangeRate, Client client, double amount)
        {
            TimestampUTC = DateTime.UtcNow;
            ClientId = client.Id;
            Client = client;
            CurrentRate = exchangeRate.Rate;
            BaseCurrency = exchangeRate.BaseCurrency;
            BaseCurrencyAmount = amount;
            ExchangedCurrency = exchangeRate.ExchangeCurrency;
            ExchangedCurrencyAmount = amount * CurrentRate;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DateTime TimestampUTC { get; set; }

        public string BaseCurrency { get; set; }

        public double BaseCurrencyAmount { get; set; }

        public string ExchangedCurrency { get; set; }

        public double ExchangedCurrencyAmount { get; set; }

        public double CurrentRate { get; set; }
    }
}
