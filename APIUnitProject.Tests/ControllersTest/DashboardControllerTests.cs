using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Dashboard.DTOs;
using MyWarehouse.Application.Dashboard.Queries;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class DashboardControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new DashboardController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetUserDashboard_Found_ReturnsOk()
        {
            // Arrange
            var expected = new DashboardConfigurationDTO { UserId = 1, Id = "dash1" };
            _mockMediator.Setup(m => m.Send(It.Is<GetDashboardByUserIdQuery>(q => q.UserId == 1), default))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetUserDashboard(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetUserDashboard_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<GetDashboardByUserIdQuery>(), default))
                .ReturnsAsync((DashboardConfigurationDTO?)null);

            // Act
            var result = await _controller.GetUserDashboard(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllUserDashboards_ReturnsOkWithList()
        {
            // Arrange
            var expected = new List<DashboardConfigurationDTO> { new() { UserId = 1, Id = "dash1" } };
            _mockMediator.Setup(m => m.Send(It.Is<GetUserDashboardsQuery>(q => q.UserId == 1), default))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetAllUserDashboards(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }
    }
}
