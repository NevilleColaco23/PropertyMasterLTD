using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.MenuPermissions;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class MenuPermissionsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly MenuPermissionsController _controller;

        public MenuPermissionsControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new MenuPermissionsController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetMenuPermissions_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetMenuPermissionsQuery();
            var expected = new Mock<IListResponseModel<GetMenuPermissionDTO>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetMenuPermissions(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetMenuPermissionById_Found_ReturnsOk()
        {
            // Arrange
            var expected = new GetMenuPermissionDTO { Id = 5 };
            _mockMediator.Setup(m => m.Send(It.Is<GetMenuPermissionByIdQuery>(q => q.Id == 5), default))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetMenuPermissionById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetMenuPermissionById_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMenuPermissionByIdQuery>(), default))
                .ReturnsAsync((GetMenuPermissionDTO?)null);

            // Act
            var result = await _controller.GetMenuPermissionById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateMenuPermission_ReturnsCreatedAtActionWithId()
        {
            // Arrange
            var command = new CreateMenuPermissionCommand { UserId = 1, MenuId = 2 };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(10);

            // Act
            var result = await _controller.CreateMenuPermission(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(10, createdResult.Value);
        }

        [Fact]
        public async Task UpdateMenuPermission_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdateMenuPermissionCommand { Id = 2 };

            // Act
            var result = await _controller.UpdateMenuPermission(1, command);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateMenuPermission_ValidId_ReturnsNoContent()
        {
            // Arrange
            var command = new UpdateMenuPermissionCommand { Id = 1 };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.UpdateMenuPermission(1, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteMenuPermission_ReturnsNoContent()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteMenuPermissionCommand>(), default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.DeleteMenuPermission(3);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
