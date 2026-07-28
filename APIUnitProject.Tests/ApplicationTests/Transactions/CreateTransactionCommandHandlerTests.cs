using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Transactions.CreateTransaction;
using MyWarehouse.Domain;
using MyWarehouse.Domain.Partners;
using MyWarehouse.Domain.Products;

namespace APIUnitProject.Tests.ApplicationTests.Transactions
{
    public class CreateTransactionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPartnerRepository> _mockPartnerRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly CreateTransactionCommandHandler _handler;

        public CreateTransactionCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPartnerRepository = new Mock<IPartnerRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork.Setup(u => u.Partners).Returns(_mockPartnerRepository.Object);
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);

            _handler = new CreateTransactionCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_PartnerNotFound_ThrowsInputValidationException()
        {
            // Arrange
            _mockPartnerRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Partner?)null);

            var command = new CreateTransactionCommand { PartnerId = 999, TransactionType = TransactionType.Sales };

            // Act & Assert
            await Assert.ThrowsAsync<InputValidationException>(() => _handler.Handle(command, CancellationToken.None));
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ProductNotFound_RollsBackTransaction()
        {
            // Arrange
            var partner = new Partner("Acme", new Address("USA", "12345", "Main St", "Springfield")) { Id = 1 };
            _mockPartnerRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(partner);
            _mockProductRepository
                .Setup(r => r.GetFiltered(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), false))
                .ReturnsAsync(new List<Product>());

            var command = new CreateTransactionCommand
            {
                PartnerId = 1,
                TransactionType = TransactionType.Sales,
                TransactionLines = new[]
                {
                    new CreateTransactionCommand.TransactionLine { ProductId = 55, ProductQuantity = 2 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InputValidationException>(() => _handler.Handle(command, CancellationToken.None));
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }
    }
}
