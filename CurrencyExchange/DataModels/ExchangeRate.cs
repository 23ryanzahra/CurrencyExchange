using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static CurrencyExchange.Helpers.EnumHelper;

namespace CurrencyExchange.DataModels
{
    public class ExchangeRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string BaseCurrency { get; set; }

        public string ExchangeCurrency { get; set; }

        public DateTime Rate { get; set; }

        public DateTime LastUpdatedUTC { get; set; }

        public RateProvider RateProvider { get; set; }
    }
}
