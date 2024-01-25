using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CurrencyExchange.Classes;

namespace CurrencyExchange.DataModels
{
    public class Trade
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DateTime TimestampUTC { get; set; }

        public double BaseCurrencyAmount { get; set; }

        public double ExchangedCurrencyAmount { get; set; }

        public double CurrentRate { get; set; }
    }
}
