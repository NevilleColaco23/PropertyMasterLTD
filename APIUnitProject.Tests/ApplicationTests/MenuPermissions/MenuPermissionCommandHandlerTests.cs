using Moq;
using MyWarehouse.Application.Common.Audit;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Common.MenuPermissions;
using MyWarehouse.Application.Dependencies.Services;
using MediatR;
using PermissionEntity = MyWarehouse.Domain.Common.Menus.MenusPermissions;

namespace APIUnitProject.Tests.ApplicationTests.MenuPermissions
{
    public class CreateMenuPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMenuPermissionRepository> _mockMenuPermissionRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly CreateMenuPermissionCommandHandler _handler;

        public CreateMenuPermissionCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMenuPermissionRepository = new Mock<IMenuPermissionRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockAuditService = new Mock<IAuditService>();

            _mockUnitOfWork.Setup(u => u.MenuPermissions).Returns(_mockMenuPermissionRepository.Object);
            _mockCurrentUserService.Setup(s => s.UserId).Returns("5");

            _handler = new CreateMenuPermissionCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_AddsPermissionAndSavesChangesAndLogsAudit()
        {
            var command = new CreateMenuPermissionCommand
            {
                UserId = 1,
                MenuId = 2,
                AccessLevel = "write",
                IsActive = true,
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(1)
            };

            PermissionEntity? added = null;
            _mockMenuPermissionRepository
                .Setup(r => r.Add(It.IsAny<PermissionEntity>(), default))
                .Callback<PermissionEntity, CancellationToken>((p, _) =>
                {
                    p.Id = 10;
                    added = p;
                })
                .ReturnsAsync((PermissionEntity p, CancellationToken _) => p);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(10, result);
            Assert.NotNull(added);
            Assert.Equal(5, added!.CreatedBy);
            _mockMenuPermissionRepository.Verify(r => r.Add(It.IsAny<PermissionEntity>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockAuditService.Verify(a => a.LogCreate("MenuPermissions", 10, added, 5, null), Times.Once);
        }
    }

    public class DeleteMenuPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMenuPermissionRepository> _mockMenuPermissionRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly DeleteMenuPermissionCommandHandler _handler;

        public DeleteMenuPermissionCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMenuPermissionRepository = new Mock<IMenuPermissionRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockAuditService = new Mock<IAuditService>();

            _mockUnitOfWork.Setup(u => u.MenuPermissions).Returns(_mockMenuPermissionRepository.Object);
            _mockCurrentUserService.Setup(s => s.UserId).Returns("5");

            _handler = new DeleteMenuPermissionCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task Handle_PermissionNotFound_DoesNotRemoveOrLog()
        {
            _mockMenuPermissionRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PermissionEntity?)null);

            var command = new DeleteMenuPermissionCommand { Id = 99 };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            _mockMenuPermissionRepository.Verify(r => r.Remove(It.IsAny<PermissionEntity>()), Times.Never);
            _mockAuditService.Verify(a => a.LogDelete(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PermissionEntity>(), It.IsAny<int>(), null), Times.Never);
        }

        [Fact]
        public async Task Handle_PermissionFound_RemovesAndSavesChangesAndLogsAudit()
        {
            var existing = new PermissionEntity { Id = 1, UserId = 2, MenuID = 3 };
            _mockMenuPermissionRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            var command = new DeleteMenuPermissionCommand { Id = 1 };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            _mockMenuPermissionRepository.Verify(r => r.Remove(existing), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockAuditService.Verify(a => a.LogDelete("MenuPermissions", 1, existing, 5, null), Times.Once);
        }
    }

    public class UpdateMenuPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMenuPermissionRepository> _mockMenuPermissionRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly UpdateMenuPermissionCommandHandler _handler;

        public UpdateMenuPermissionCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMenuPermissionRepository = new Mock<IMenuPermissionRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockAuditService = new Mock<IAuditService>();

            _mockUnitOfWork.Setup(u => u.MenuPermissions).Returns(_mockMenuPermissionRepository.Object);
            _mockCurrentUserService.Setup(s => s.UserId).Returns("5");

            _handler = new UpdateMenuPermissionCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesPermissionAndSavesChangesAndLogsAudit()
        {
            var oldPermission = new PermissionEntity { Id = 1, UserId = 2, MenuID = 3, AccessLevel = "read" };
            _mockMenuPermissionRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(oldPermission);
            _mockMenuPermissionRepository
                .Setup(r => r.Update(It.IsAny<PermissionEntity>(), default))
                .ReturnsAsync((PermissionEntity p, CancellationToken _) => p);

            var command = new UpdateMenuPermissionCommand
            {
                Id = 1,
                UserId = 2,
                MenuId = 3,
                AccessLevel = "write",
                IsActive = true,
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(1)
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            _mockMenuPermissionRepository.Verify(r => r.Update(It.Is<PermissionEntity>(p => p.AccessLevel == "write" && p.UpdatedBy == 5), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockAuditService.Verify(a => a.LogUpdate("MenuPermissions", 1, oldPermission, It.IsAny<PermissionEntity>(), 5, null), Times.Once);
        }
    }
}
