using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.Partners.DeletePartner;
using MyWarehouse.Application.Partners.GetPartnerDetails;
using MyWarehouse.Application.Partners.GetPartnersList;
using MyWarehouse.Application.Partners.UpdatePartner;
using MyWarehouse.Infrastructure.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class PartnerControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PartnerController _controller;

        public PartnerControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new PartnerController(_mockMediator.Object);
        }

        [Fact]
        public async Task Create_ReturnsOkWithId()
        {
            // Arrange
            var command = new CreatePartnerCommand
            {
                Name = "Acme",
                Address = new CreatePartnerCommand.AddressDto
                {
                    Country = "USA",
                    ZipCode = "12345",
                    Street = "Main St",
                    City = "Springfield"
                }
            };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(7);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(7, okResult.Value);
        }

        [Fact]
        public async Task GetList_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new ListQueryModel<PartnerDto>();
            var expected = new Mock<IListResponseModel<PartnerDto>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetList(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task Get_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var expected = new PartnerDetailsDto { Id = 5, Name = "Acme" };
            _mockMediator.Setup(m => m.Send(It.Is<GetPartnerDetailsQuery>(q => q.Id == 5), default))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Get(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<DeletePartnerCommand>(), default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockMediator.Verify(m => m.Send(It.Is<DeletePartnerCommand>(c => c.Id == 3), default), Times.Once);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdatePartnerCommand { Id = 2, Name = "Acme" };

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockMediator.Verify(m => m.Send(It.IsAny<UpdatePartnerCommand>(), default), Times.Never);
        }

        [Fact]
        public async Task Update_ValidId_ReturnsNoContent()
        {
            // Arrange
            var command = new UpdatePartnerCommand { Id = 1, Name = "Acme" };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
