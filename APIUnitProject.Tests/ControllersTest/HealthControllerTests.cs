using Microsoft.AspNetCore.Mvc;
using MyWarehouse.WebApi.API;

namespace APIUnitProject.Tests.ControllersTest
{
    public class HealthControllerTests
    {
        [Fact]
        public void Get_ReturnsOkWithHealthyStatus()
        {
            // Arrange
            var controller = new HealthController();

            // Act
            var result = controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var value = okResult.Value!;
            var statusProp = value.GetType().GetProperty("Status");
            var serviceProp = value.GetType().GetProperty("Service");

            Assert.Equal("Healthy", statusProp!.GetValue(value));
            Assert.Equal("WebApi", serviceProp!.GetValue(value));
        }
    }
}
