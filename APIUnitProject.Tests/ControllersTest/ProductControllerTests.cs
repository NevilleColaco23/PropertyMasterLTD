using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Products.CreateProduct;
using MyWarehouse.Application.Products.DeleteProduct;
using MyWarehouse.Application.Products.GetProduct;
using MyWarehouse.Application.Products.GetProductsList;
using MyWarehouse.Application.Products.GetProductsSummary;
using MyWarehouse.Application.Products.ProductStockMass;
using MyWarehouse.Application.Products.ProductStockValue;
using MyWarehouse.Application.Products.UpdateProduct;
using MyWarehouse.Infrastructure.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class ProductControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new ProductController(_mockMediator.Object);
        }

        [Fact]
        public async Task Create_ReturnsOkWithId()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Widget",
                Description = "A widget",
                MassValue = 1.5f,
                MassUnitSymbol = "kg",
                PriceAmount = 9.99m,
                PriceCurrencyCode = "AUD"
            };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(11);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(11, okResult.Value);
        }

        [Fact]
        public async Task GetList_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetProductsListQuery();
            var expected = new Mock<IListResponseModel<ProductDto>>().Object;
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
            var expected = new ProductDetailsDto { Id = 5, Name = "Widget" };
            _mockMediator.Setup(m => m.Send(It.Is<GetProductDetailsQuery>(q => q.Id == 5), default))
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
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdateProductCommand { Id = 2, Name = "Widget", Description = "d" };

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Update_ValidId_ReturnsNoContent()
        {
            // Arrange
            var command = new UpdateProductCommand { Id = 1, Name = "Widget", Description = "d" };
            _mockMediator.Setup(m => m.Send(command, default)).ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ProductStockMass_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var expected = new StockMassDto { Value = 100, Unit = "kg" };
            _mockMediator.Setup(m => m.Send(It.IsAny<ProductStockMassQuery>(), default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.ProductStockMass();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task ProductStockValue_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var expected = new StockValueDto { Amount = 500m, CurrencyCode = "AUD" };
            _mockMediator.Setup(m => m.Send(It.IsAny<ProductStockValueQuery>(), default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.ProductStockValue();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task ProductStockCount_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var expected = new ProductStockCountDto { ProductCount = 3, TotalStock = 40 };
            _mockMediator.Setup(m => m.Send(It.IsAny<ProductStockCountQuery>(), default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.ProductStockCount();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }
    }
}
