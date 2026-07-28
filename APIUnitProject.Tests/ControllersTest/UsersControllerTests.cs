using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Users.DTO;
using MyWarehouse.Application.Common.Users;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new UsersController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var expected = new Mock<IListResponseModel<GetUserDTO>>().Object;
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }
    }
}
