using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Products.CreateProduct;
using MyWarehouse.Application.Products.DeleteProduct;
using MyWarehouse.Domain.Products;

namespace APIUnitProject.Tests.ApplicationTests.Products
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);

            _handler = new CreateProductCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ThrowsArgumentException_DueToHardcodedZeroMass()
        {
            // NOTE: CreateProductCommandHandler currently constructs the Product with a hardcoded
            // `new Mass(0, MassUnit.Gram)` regardless of the request's MassValue. Since the domain's
            // minimum required mass is 0.1, this always throws today. This test documents the
            // current (buggy) behavior; once the handler is fixed to use request.MassValue, this
            // test should be updated to assert successful creation instead.

            // Arrange
            var command = new CreateProductCommand
            {
                Name = "  Widget  ",
                Description = "  A widget  ",
                MassValue = 1.5f,
                MassUnitSymbol = "kg",
                PriceAmount = 9.99m,
                PriceCurrencyCode = "AUD"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            _mockProductRepository.Verify(r => r.Add(It.IsAny<Product>(), default), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Never);
        }
    }

    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly DeleteProductCommandHandler _handler;

        public DeleteProductCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);

            _handler = new DeleteProductCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

            var command = new DeleteProductCommand { Id = 123 };

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            _mockProductRepository.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
        }
    }
}
