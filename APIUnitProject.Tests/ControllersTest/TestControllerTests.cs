using Messaging.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using MyWarehouse.WebApi.API.Test;

namespace APIUnitProject.Tests.ControllersTest
{
    public class TestControllerTests
    {
        private readonly Mock<IOptions<RabbitMqOptions>> _mockOptions;
        private readonly TestController _controller;

        public TestControllerTests()
        {
            _mockOptions = new Mock<IOptions<RabbitMqOptions>>();
            _mockOptions.Setup(o => o.Value).Returns(new RabbitMqOptions());

            _controller = new TestController(_mockOptions.Object);
        }

        [Fact]
        public void GetTest_ReturnsOkWithMessage()
        {
            // Act
            var result = _controller.GetTest();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var messageProp = okResult.Value!.GetType().GetProperty("message");
            Assert.Equal("Test endpoint is working!", messageProp!.GetValue(okResult.Value));
        }
    }
}
