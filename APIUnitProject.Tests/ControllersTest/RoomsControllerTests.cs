using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Driver;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class RoomsControllerTests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly RoomsController _controller;

        public RoomsControllerTests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _controller = new RoomsController(_mockDb.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetByProperty_InvalidPropertyId_ReturnsBadRequest(int propertyId)
        {
            // Act
            var result = await _controller.GetByProperty(propertyId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("propertyId is required", badRequestResult.Value);

            // The Mongo database should never be queried for an invalid propertyId
            _mockDb.Verify(db => db.GetCollection<MongoDB.Bson.BsonDocument>(It.IsAny<string>(), null), Times.Never);
        }
    }
}
