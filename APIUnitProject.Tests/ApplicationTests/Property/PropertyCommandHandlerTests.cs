using Moq;
using MyWarehouse.Application.Common.Audit;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Property.CreateProperty;
using MyWarehouse.Application.Property.DeleteProperty;
using MyWarehouse.Application.Property.UpdateProperty;
using PropEntity = MyWarehouse.Domain.Property.Property;
using RoomEntity = MyWarehouse.Domain.Property.Room;
using PropertyAuditEntity = MyWarehouse.Domain.Property.PropertyAudit;
using CreateRoomDto = MyWarehouse.Application.Property.CreateProperty.RoomDto;
using UpdateRoomDto = MyWarehouse.Application.Property.UpdateProperty.RoomDto;

namespace APIUnitProject.Tests.ApplicationTests.PropertyHandlers
{
    public class CreatePropertyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPropertyRepository> _mockPropertyRepository;
        private readonly Mock<IRoomRepository> _mockRoomRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly CreatePropertyCommandHandler _handler;

        public CreatePropertyCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockRoomRepository = new Mock<IRoomRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockAuditService = new Mock<IAuditService>();

            _mockUnitOfWork.Setup(u => u.Properties).Returns(_mockPropertyRepository.Object);
            _mockUnitOfWork.Setup(u => u.Rooms).Returns(_mockRoomRepository.Object);
            _mockCurrentUserService.Setup(s => s.UserId).Returns("1");

            _handler = new CreatePropertyCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task Handle_NoRooms_ThrowsInvalidOperationException()
        {
            var command = new CreatePropertyCommand(true)
            {
                Name = "Test Property",
                Rooms = new List<CreateRoomDto>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
            _mockPropertyRepository.Verify(r => r.Add(It.IsAny<PropEntity>(), default), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCommand_AddsPropertyAndRoomsAndSavesChanges()
        {
            var command = new CreatePropertyCommand(true)
            {
                Name = "Test Property",
                Rooms = new List<CreateRoomDto>
                {
                    new() { RoomCode = "R1", RoomName = "Room 1", Active = true }
                }
            };

            PropEntity? addedProperty = null;
            _mockPropertyRepository
                .Setup(r => r.Add(It.IsAny<PropEntity>(), default))
                .Callback<PropEntity, CancellationToken>((p, _) => addedProperty = p)
                .ReturnsAsync((PropEntity p, CancellationToken _) => p);

            _mockRoomRepository
                .Setup(r => r.Add(It.IsAny<RoomEntity>(), default))
                .ReturnsAsync((RoomEntity r, CancellationToken _) => r);

            _mockPropertyRepository
                .Setup(r => r.Update(It.IsAny<PropEntity>(), default))
                .ReturnsAsync((PropEntity p, CancellationToken _) => p);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(addedProperty);
            Assert.Equal("Test Property", addedProperty!.Name);
            _mockPropertyRepository.Verify(r => r.Add(It.IsAny<PropEntity>(), default), Times.Once);
            _mockRoomRepository.Verify(r => r.Add(It.IsAny<RoomEntity>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.AtLeastOnce);
        }
    }

    public class DeletePropertyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPropertyRepository> _mockPropertyRepository;
        private readonly Mock<IRoomRepository> _mockRoomRepository;
        private readonly Mock<IPropertyAuditRepository> _mockPropertyAuditRepository;
        private readonly Mock<IRoomAuditRepository> _mockRoomAuditRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly DeletePropertyCommandHandler _handler;

        public DeletePropertyCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockRoomRepository = new Mock<IRoomRepository>();
            _mockPropertyAuditRepository = new Mock<IPropertyAuditRepository>();
            _mockRoomAuditRepository = new Mock<IRoomAuditRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockAuditService = new Mock<IAuditService>();

            _mockUnitOfWork.Setup(u => u.Properties).Returns(_mockPropertyRepository.Object);
            _mockUnitOfWork.Setup(u => u.Rooms).Returns(_mockRoomRepository.Object);
            _mockUnitOfWork.Setup(u => u.PropertyAudits).Returns(_mockPropertyAuditRepository.Object);
            _mockUnitOfWork.Setup(u => u.RoomAudits).Returns(_mockRoomAuditRepository.Object);
            _mockCurrentUserService.Setup(s => s.UserId).Returns("1");

            _handler = new DeletePropertyCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task Handle_PropertyNotFound_ThrowsKeyNotFoundException()
        {
            _mockPropertyRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PropEntity?)null);

            var command = new DeletePropertyCommand { Id = 99 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ExistingProperty_SoftDeletesAndSavesChanges()
        {
            var property = new PropEntity("Test Property", true, new List<PropEntity.Room>())
            {
                Id = 1
            };
            _mockPropertyRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(property);
            _mockRoomRepository.Setup(r => r.GetRoomsByPropertyIdAsync(1)).ReturnsAsync(new List<RoomEntity>());

            var command = new DeletePropertyCommand { Id = 1 };

            await _handler.Handle(command, CancellationToken.None);

            _mockPropertyRepository.Verify(r => r.Update(It.IsAny<PropEntity>(), default), Times.Once);
            _mockPropertyAuditRepository.Verify(r => r.Add(It.IsAny<PropertyAuditEntity>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.AtLeastOnce);
        }
    }

    public class UpdatePropertyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPropertyRepository> _mockPropertyRepository;
        private readonly Mock<IRoomRepository> _mockRoomRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly UpdatePropertyCommandHandler _handler;

        public UpdatePropertyCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockRoomRepository = new Mock<IRoomRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockAuditService = new Mock<IAuditService>();

            _mockUnitOfWork.Setup(u => u.Properties).Returns(_mockPropertyRepository.Object);
            _mockUnitOfWork.Setup(u => u.Rooms).Returns(_mockRoomRepository.Object);
            _mockCurrentUserService.Setup(s => s.UserId).Returns("1");

            _handler = new UpdatePropertyCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task Handle_NoRooms_ThrowsInvalidOperationException()
        {
            var command = new UpdatePropertyCommand
            {
                Id = 1,
                Name = "Test Property",
                Rooms = new List<UpdateRoomDto>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PropertyNotFound_ThrowsKeyNotFoundException()
        {
            var command = new UpdatePropertyCommand
            {
                Id = 99,
                Name = "Test Property",
                Rooms = new List<UpdateRoomDto>
                {
                    new() { RoomCode = "R1", RoomName = "Room 1", Active = true }
                }
            };

            _mockPropertyRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PropEntity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesPropertyAndSavesChanges()
        {
            var existingProperty = new PropEntity("Old Name", true, new List<PropEntity.Room>())
            {
                Id = 1
            };

            _mockPropertyRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingProperty);
            _mockRoomRepository.Setup(r => r.GetRoomsByPropertyIdAsync(1)).ReturnsAsync(new List<RoomEntity>());
            _mockPropertyRepository
                .Setup(r => r.Update(It.IsAny<PropEntity>(), default))
                .ReturnsAsync((PropEntity p, CancellationToken _) => p);

            var command = new UpdatePropertyCommand
            {
                Id = 1,
                Name = "New Name",
                Rooms = new List<UpdateRoomDto>
                {
                    new() { Id = 0, RoomCode = "R1", RoomName = "Room 1", Active = true }
                }
            };

            await _handler.Handle(command, CancellationToken.None);

            _mockPropertyRepository.Verify(r => r.Update(It.IsAny<PropEntity>(), default), Times.Once);
            _mockRoomRepository.Verify(r => r.Add(It.IsAny<RoomEntity>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.AtLeastOnce);
        }
    }
}
