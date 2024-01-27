using CurrencyExchange.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly DataContext _dataContext;
        private readonly IMemoryCache _cache;


        public ClientController(ILogger<ClientController> logger, DataContext dataContext, IMemoryCache cache)
        {
            _logger = logger;
            _dataContext = dataContext;
            _cache = cache;
        }

        [HttpPost]
        [Route("[action]")]
        public ActionResult<Client> AddNew(string firstName, string lastName)
        {
            using (var transaction = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    var client = _dataContext.Clients.FirstOrDefault(x => x.FirstName == firstName && x.LastName == lastName);

                    if (client != null)
                    {
                        _logger.LogInformation($"Client: {firstName} {lastName} has already been created.");
                        return BadRequest("No clients with duplicate details are allowed.");
                    }

                    client = new DataModels.Client(firstName, lastName);
                    _dataContext.Clients.Add(client);
                    _dataContext.SaveChanges();
                    transaction.Commit();
                    _logger.LogInformation($"Client: {firstName} {lastName} has been created successfully.");
                    return Ok(client);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "An error occurred while processing your request.");
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<int> GetClientNumberOfTradesInLimitValidtityPeriod(int clientId)
        {
            try
            {
                var client = _dataContext.Clients.FirstOrDefault(x => x.Id == clientId);
                if (client == null)
                {
                    _logger.LogInformation($"Client with Id: {clientId} not found.");
                    return BadRequest("No data was found.");
                }

                var count = _cache.Get<List<DateTime>>(clientId)?.Count() ?? 0;
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<Dictionary<int, string>?> GetAll()
        {
            try
            {
                return Ok(_dataContext.Clients?.ToDictionary(x => x.Id, x => $"{x.FirstName} {x.LastName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing your request.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
