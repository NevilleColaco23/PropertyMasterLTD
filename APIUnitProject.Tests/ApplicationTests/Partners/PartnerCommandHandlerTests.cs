using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.Partners.DeletePartner;
using MyWarehouse.Domain.Partners;

namespace APIUnitProject.Tests.ApplicationTests.Partners
{
    public class CreatePartnerCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPartnerRepository> _mockPartnerRepository;
        private readonly CreatePartnerCommandHandler _handler;

        public CreatePartnerCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPartnerRepository = new Mock<IPartnerRepository>();
            _mockUnitOfWork.Setup(u => u.Partners).Returns(_mockPartnerRepository.Object);

            _handler = new CreatePartnerCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_AddsPartnerAndSavesChanges()
        {
            // Arrange
            var command = new CreatePartnerCommand
            {
                Name = "  Acme Corp  ",
                Address = new CreatePartnerCommand.AddressDto
                {
                    Country = "USA",
                    ZipCode = "12345",
                    Street = "Main St",
                    City = "Springfield"
                }
            };

            Partner? addedPartner = null;
            _mockPartnerRepository
                .Setup(r => r.Add(It.IsAny<Partner>(), default))
                .Callback<Partner, CancellationToken>((p, _) => addedPartner = p)
                .ReturnsAsync((Partner p, CancellationToken _) => p);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(addedPartner);
            Assert.Equal("Acme Corp", addedPartner!.Name);
            _mockPartnerRepository.Verify(r => r.Add(It.IsAny<Partner>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            Assert.Equal(addedPartner.Id, result);
        }
    }

    public class DeletePartnerCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPartnerRepository> _mockPartnerRepository;
        private readonly DeletePartnerCommandHandler _handler;

        public DeletePartnerCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPartnerRepository = new Mock<IPartnerRepository>();
            _mockUnitOfWork.Setup(u => u.Partners).Returns(_mockPartnerRepository.Object);

            _handler = new DeletePartnerCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ExistingPartner_RemovesPartnerAndSavesChanges()
        {
            // Arrange
            var partner = new Partner("Acme", new Address("USA", "12345", "Main St", "Springfield")) { Id = 1 };
            _mockPartnerRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(partner);

            var command = new DeletePartnerCommand { Id = 1 };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockPartnerRepository.Verify(r => r.Remove(partner), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task Handle_PartnerNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            _mockPartnerRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Partner?)null);

            var command = new DeletePartnerCommand { Id = 999 };

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            _mockPartnerRepository.Verify(r => r.Remove(It.IsAny<Partner>()), Times.Never);
        }
    }
}
