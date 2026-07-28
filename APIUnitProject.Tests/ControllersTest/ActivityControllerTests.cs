using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Application.UserActivity.Queries;
using MyWarehouse.Domain.UserActivity;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class ActivityControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly ActivityController _controller;

        public ActivityControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new ActivityController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetRecentActivities_ReturnsOkWithMediatorResult()
        {
            var expected = new List<UserActivityDTO> { new UserActivityDTO { ActivityId = 1 } };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRecentActivitiesQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetRecentActivities(50);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetUserActivities_ReturnsOkWithMediatorResult()
        {
            var expected = new List<UserActivityDTO> { new UserActivityDTO { ActivityId = 2, UserId = 5 } };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserActivitiesQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetUserActivities(5, null, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetActivitiesByType_ReturnsOkWithMediatorResult()
        {
            var expected = new List<UserActivityDTO>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivitiesByTypeQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetActivitiesByType(ActivityType.Login, null, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetEntityActivities_ReturnsOkWithMediatorResult()
        {
            var expected = new List<UserActivityDTO>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetEntityActivitiesQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetEntityActivities("Property", 10, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetActivitiesPaged_ReturnsOkWithMediatorResult()
        {
            var expected = new PagedActivitiesDTO { Page = 1, PageSize = 50, TotalCount = 0 };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivitiesPagedQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetActivitiesPaged();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetStatistics_ReturnsOkWithMediatorResult()
        {
            var expected = new ActivityStatisticsDTO { TotalActivities = 42 };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivityStatisticsQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetStatistics(null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetSummary_ReturnsOkWithMediatorResult()
        {
            var expected = new ActivitySummaryDTO { TotalToday = 3 };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivitySummaryQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetSummary(10, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetWidgetData_ReturnsOkWithMediatorResult()
        {
            var expected = new List<ActivityWidgetDTO>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivityWidgetDataQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetWidgetData(15);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }
    }
}
