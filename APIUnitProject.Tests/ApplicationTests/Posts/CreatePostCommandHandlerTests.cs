using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Posts.CreatePost;
using MyWarehouse.Domain.Posts;
using UserEntity = MyWarehouse.Domain.Users.Users;

namespace APIUnitProject.Tests.ApplicationTests.Posts
{
    public class CreatePostCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUsersRepository> _mockUsersRepository;
        private readonly Mock<IPostsRepository> _mockPostsRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly CreatePostCommandHandler _handler;

        public CreatePostCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUsersRepository = new Mock<IUsersRepository>();
            _mockPostsRepository = new Mock<IPostsRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();

            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUsersRepository.Object);
            _mockUnitOfWork.Setup(u => u.Posts).Returns(_mockPostsRepository.Object);

            _handler = new CreatePostCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_AddsPostAndReturnsId()
        {
            _mockCurrentUserService.Setup(s => s.UserId).Returns("7");
            var user = new UserEntity("johndoe", "john@example.com", "P@ssw0rd", "1234567890") { Id = 7, Alias = "jdoe" };
            _mockUsersRepository
                .Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(user);

            Post? addedPost = null;
            _mockPostsRepository
                .Setup(r => r.Add(It.IsAny<Post>(), default))
                .Callback<Post, CancellationToken>((p, _) => addedPost = p)
                .ReturnsAsync((Post p, CancellationToken _) =>
                {
                    p.Id = 99;
                    return p;
                });

            var command = new CreatePostCommand
            {
                AuthorName = "John Doe",
                Text = "Hello @world, this is a test post!"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(99, result);
            Assert.NotNull(addedPost);
            Assert.Equal(7, addedPost!.UserId);
            Assert.Equal("jdoe", addedPost.AuthorAlias);
            Assert.Equal("John Doe", addedPost.AuthorName);
            Assert.Contains("world", addedPost.Mentions);
            _mockPostsRepository.Verify(r => r.Add(It.IsAny<Post>(), default), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_StillCreatesPostWithoutAlias()
        {
            _mockCurrentUserService.Setup(s => s.UserId).Returns("123");
            _mockUsersRepository
                .Setup(r => r.GetByIdAsync(123))
                .ReturnsAsync((UserEntity?)null);

            Post? addedPost = null;
            _mockPostsRepository
                .Setup(r => r.Add(It.IsAny<Post>(), default))
                .Callback<Post, CancellationToken>((p, _) => addedPost = p)
                .ReturnsAsync((Post p, CancellationToken _) =>
                {
                    p.Id = 1;
                    return p;
                });

            var command = new CreatePostCommand
            {
                AuthorName = "Jane Doe",
                Text = "No mentions here"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(1, result);
            Assert.NotNull(addedPost);
            Assert.Null(addedPost!.AuthorAlias);
            Assert.Empty(addedPost.PosterPropertyIds);
        }

        [Fact]
        public async Task Handle_InvalidUserId_DefaultsToZero()
        {
            _mockCurrentUserService.Setup(s => s.UserId).Returns("not-a-number");
            _mockUsersRepository
                .Setup(r => r.GetByIdAsync(0))
                .ReturnsAsync((UserEntity?)null);

            Post? addedPost = null;
            _mockPostsRepository
                .Setup(r => r.Add(It.IsAny<Post>(), default))
                .Callback<Post, CancellationToken>((p, _) => addedPost = p)
                .ReturnsAsync((Post p, CancellationToken _) => p);

            var command = new CreatePostCommand
            {
                AuthorName = "Anon",
                Text = "text"
            };

            await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(addedPost);
            Assert.Equal(0, addedPost!.UserId);
        }
    }
}
