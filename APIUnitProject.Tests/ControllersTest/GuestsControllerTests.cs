using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Driver;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class GuestsControllerTests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly GuestsController _controller;

        public GuestsControllerTests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _controller = new GuestsController(_mockDb.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        public async Task Search_QueryTooShortOrEmpty_ReturnsOkWithEmptyList(string? query)
        {
            // Act
            var result = await _controller.Search(query!, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<System.Collections.Generic.List<object>>(okResult.Value);
            Assert.Empty(list);

            // The Mongo database should never be queried for invalid search terms
            _mockDb.Verify(db => db.GetCollection<MongoDB.Bson.BsonDocument>(It.IsAny<string>(), null), Times.Never);
        }
    }
}
