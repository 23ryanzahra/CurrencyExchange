using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyExchange.DataModels
{
    public class Trade
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }


        public int ExchangeRateId { get; set; }
        public ExchangeRate ExchangeRate { get; set; }

        public DateTime TimestampUTC { get; set; }

        public decimal BaseCurrencyAmount { get; set; }

        public decimal ExchangedCurrencyAmount { get; set; }

        public decimal CurrentRate { get; set; }
    }
}
