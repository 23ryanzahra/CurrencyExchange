using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyExchange.DataModels
{
    public class Client
    {
        public Client( string? firstName = null, string? lastName = null)
        {
            FirstName = firstName ?? string.Empty;
            LastName = lastName ?? string.Empty;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

    }
}
