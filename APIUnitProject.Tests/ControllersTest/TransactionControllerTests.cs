using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Transactions.CreateTransaction;
using MyWarehouse.Application.Transactions.GetTransactionDetails;
using MyWarehouse.Application.Transactions.GetTransactionsList;
using MyWarehouse.Infrastructure.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class TransactionControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly TransactionController _controller;

        public TransactionControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new TransactionController(_mockMediator.Object);
        }

        [Fact]
        public async Task Create_ReturnsOkWithId()
        {
            // Arrange
            var command = new CreateTransactionCommand { PartnerId = 1 };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(20);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(20, okResult.Value);
        }

        [Fact]
        public async Task GetList_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetTransactionListQuery();
            var expected = new Mock<IListResponseModel<TransactionDto>>().Object;
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
            var expected = new TransactionDetailsDto { Id = 9 };
            _mockMediator.Setup(m => m.Send(It.Is<GetTransactionDetailsQuery>(q => q.Id == 9), default))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Get(9);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }
    }
}
