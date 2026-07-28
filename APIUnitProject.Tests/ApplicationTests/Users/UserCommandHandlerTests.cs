using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Users.CreateUser;
using MyWarehouse.Application.Users.GetUsers;
using UserEntity = MyWarehouse.Domain.Users.Users;

namespace APIUnitProject.Tests.ApplicationTests.Users
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUsersRepository> _mockUsersRepository;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUsersRepository = new Mock<IUsersRepository>();
            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUsersRepository.Object);

            _handler = new CreateUserCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_AddsUserAndSavesChanges()
        {
            var command = new CreateUserCommand
            {
                UserName = " testuser ",
                Email = " test@example.com ",
                Password = "P@ssw0rd",
                PhoneNumber = "1234567890"
            };

            UserEntity? addedUser = null;
            _mockUsersRepository
                .Setup(r => r.Add(It.IsAny<UserEntity>(), default))
                .Callback<UserEntity, CancellationToken>((u, _) => addedUser = u)
                .ReturnsAsync((UserEntity u, CancellationToken _) =>
                {
                    u.Id = 42;
                    return u;
                });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(42, result);
            Assert.NotNull(addedUser);
            Assert.Equal("testuser", addedUser!.UserName);
            Assert.Equal("test@example.com", addedUser.Email);
            _mockUsersRepository.Verify(r => r.Add(It.IsAny<UserEntity>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }

    public class GetUsersByUseridQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUsersRepository> _mockUsersRepository;
        private readonly GetUsersByUseridQueryHandler _handler;

        public GetUsersByUseridQueryHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUsersRepository = new Mock<IUsersRepository>();
            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUsersRepository.Object);

            _handler = new GetUsersByUseridQueryHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ReturnsResponseModel_WithExpectedPaging()
        {
            var query = new GetUsersByUseridQuery { PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(10, result.PageSize);
            Assert.NotNull(result.Results);
        }
    }
}
