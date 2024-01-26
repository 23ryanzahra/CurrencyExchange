using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Classes
{
    public class TradeOrder
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public string BaseCurrency { get; set; }

        [Required]
        public string ExchangeCurrency { get; set; }

        [Required]
        public double Amount { get;set; }
    }
}
