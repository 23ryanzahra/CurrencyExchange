namespace CurrencyExchange.Classes
{
    public class FixerExchangeRate
    {
        public bool Success { get; set; }

        public long Timestamp { get; set; }

        public string Base { get; set; }

        public DateTimeOffset Date { get; set; }

        public Dictionary<string, double> Rates { get; set; }
    }
}
