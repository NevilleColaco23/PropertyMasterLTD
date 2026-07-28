using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Menus;
using MyWarehouse.Application.Common.Menus.DTO;
using MyWarehouse.Application.Common.Searchbox;
using MyWarehouse.WebApi.API.DomainControllers;

namespace APIUnitProject.Tests.ControllersTest
{
    public class MenuControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly MenuController _controller;

        public MenuControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new MenuController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetListByUserId_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetMenuListQuery { userId = 1 };
            var expected = new Mock<IListResponseModel<GetMenuPermissionMappingListDTO>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetListByUserId(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
            _mockMediator.Verify(m => m.Send(query, default), Times.Once);
        }

        [Fact]
        public async Task GetSearchResults_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetSearchResultsQuery();
            var expected = new Mock<IListResponseModel<GetSearchResultsDTO>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetSearchResults(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetAllMenus_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetAllMenusQuery();
            var expected = new Mock<IListResponseModel<GetAllMenusDTO>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetAllMenus(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }
    }
}
