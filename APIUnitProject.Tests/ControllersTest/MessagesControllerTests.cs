using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Messages;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class MessagesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly MessagesController _controller;

        public MessagesControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new MessagesController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetSystemMessages_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetSystemMessagesForSnackbarQuery();
            var expected = new Mock<IListResponseModel<GetSystemMessagesDTO>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetSystemMessages(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }
    }
}
