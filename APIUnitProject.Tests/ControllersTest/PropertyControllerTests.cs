using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Property.CreateProperty;
using MyWarehouse.Application.Property.DeleteProperty;
using MyWarehouse.Application.Property.GetProperty;
using MyWarehouse.Application.Property.UpdateProperty;
using MyWarehouse.Infrastructure.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class PropertyControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PropertyController _controller;

        public PropertyControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new PropertyController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetList_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetPropertyListQuery();
            var expected = new Mock<IListResponseModel<GetPropertyDto>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetList(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetList_MediatorThrows_ReturnsBadRequest()
        {
            // Arrange
            var query = new GetPropertyListQuery();
            _mockMediator.Setup(m => m.Send(query, default)).ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            var result = await _controller.GetList(query);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_PropertyFound_ReturnsOk()
        {
            // Arrange
            var expectedList = new ListResponseModel<GetPropertyDto>
            {
                Results = new List<GetPropertyDto> { new() { Id = 5, Name = "Test" } }
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPropertyListQuery>(), default)).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<GetPropertyDto>(okResult.Value);
            Assert.Equal(5, dto.Id);
        }

        [Fact]
        public async Task GetById_PropertyNotFound_ReturnsNotFound()
        {
            // Arrange
            var expectedList = new ListResponseModel<GetPropertyDto> { Results = new List<GetPropertyDto>() };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPropertyListQuery>(), default)).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionWithId()
        {
            // Arrange
            var command = new CreatePropertyCommand(isActive: true) { Name = "New Property" };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(42);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(42, createdResult.Value);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdatePropertyCommand { Id = 2, Name = "Test" };

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockMediator.Verify(m => m.Send(It.IsAny<UpdatePropertyCommand>(), default), Times.Never);
        }

        [Fact]
        public async Task Update_ValidId_ReturnsNoContent()
        {
            // Arrange
            var command = new UpdatePropertyCommand { Id = 1, Name = "Test" };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<DeletePropertyCommand>(), default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockMediator.Verify(m => m.Send(It.Is<DeletePropertyCommand>(c => c.Id == 3), default), Times.Once);
        }
    }
}
