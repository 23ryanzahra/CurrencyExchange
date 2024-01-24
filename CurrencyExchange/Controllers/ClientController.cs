using CurrencyExchange.DataModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace CurrencyExchange.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly DataContext _dataContext;
        private readonly int ClientTradeLimit = 10;
        private readonly int ClientTradeLimitValidtityPeriod = 10;


        public ClientController(ILogger<ClientController> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [HttpPost(Name = "AddNewClient")]
        public ActionResult AddNewClient(string firstName, string lastName)
        {
            using (var transaction = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    var client = _dataContext.Clients.FirstOrDefault(x => x.FirstName == firstName && x.LastName == lastName);

                    if (client != null) { return BadRequest("No clients with duplicate details are allowed."); }

                    client = new DataModels.Client(firstName, lastName);
                    _dataContext.Clients.Add(client);
                    _dataContext.SaveChanges();
                    _logger.LogInformation($"Client: {firstName} {lastName} has been created successfully.");
                    return Ok(client.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing your request.");
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }
        }

        [HttpGet(Name = "GetNumberOfTradesInLimitValidtityPeriod")]
        public ActionResult GetNumberOfTradesInLimitValidtityPeriod(int clientId)
        {
            try
            {
                var client = _dataContext.Clients.FirstOrDefault(x => x.Id == clientId);
                if (client == null) { return BadRequest("No data was found."); }

                var count = _dataContext.Trades.Count(x => x.ClientId == clientId && x.TimestampUTC >= DateTime.UtcNow.AddMinutes(-1 * ClientTradeLimitValidtityPeriod));
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
