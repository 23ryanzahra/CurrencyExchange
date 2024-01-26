using System.Data;

namespace Tests
{
    public class ClientControllerTests
    {
        private readonly Mock<ILogger<ClientController>> _mockLogger;
        private Helper _helper { get; set; } = new Helper();

        public ClientControllerTests()
        {
            _mockLogger = new Mock<ILogger<ClientController>>();
        }


        [Fact]
        public void AddNew_ReturnsBadRequest_WhenClientExists()
        {
            //initialize cache and datacontext
            var cache = _helper.CreateMemoryCache();
            using var context = _helper.CreateDataContext(databaseName: "AddNew_ReturnsBadRequest_WhenClientExists");

            //Arrange test data
            var existingClient = new Client("John", "Doe");
            context.Clients.Add(existingClient);
            context.SaveChanges();

            //Initialize Controller
            var controller = new ClientController(_mockLogger.Object, context, cache);

            // Act
            var result = controller.AddNew("John", "Doe");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void GetAll_ReturnsAllClients()
        {
            //initialize cache and datacontext
            var cache = _helper.CreateMemoryCache();
            using var context = _helper.CreateDataContext(databaseName: "GetAll_ReturnsAllClients");

            //Arrange test data
            context.Clients.AddRange(new[] { new Client("John", "Doe"), new Client("Jane", "Doe") });
            context.SaveChanges();

            //Initialize Controller
            var controller = new ClientController(_mockLogger.Object, context, cache);

            // Act
            var result = controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Dictionary<int, string>>(okResult.Value);

            var expectedResult = new Dictionary<int, string>
            {
                { 1, "John Doe" },
                { 2, "Jane Doe" }
            };

            Assert.Equal(expectedResult, returnValue);
        }      
    }
}