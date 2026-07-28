using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Application.UserActivity.Queries;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class ActivityAnalyticsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly ActivityAnalyticsController _controller;

        public ActivityAnalyticsControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new ActivityAnalyticsController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetAnalyticsSummary_ReturnsOkWithMediatorResult()
        {
            var expected = new ActivityAnalyticsSummaryDto();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivityAnalyticsSummaryQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetAnalyticsSummary(null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetTopActiveUsers_ReturnsOkWithMediatorResult()
        {
            var expected = new List<UserActivityStatsDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetTopActiveUsersQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetTopActiveUsers(10, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetActivityDistribution_ReturnsOkWithMediatorResult()
        {
            var expected = new List<ActivityTypeDistributionDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivityTypeDistributionQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetActivityDistribution(null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetMostAccessedEntities_ReturnsOkWithMediatorResult()
        {
            var expected = new List<EntityAccessStatsDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMostAccessedEntitiesQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetMostAccessedEntities(null, 10, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetPeakUsageTimes_ReturnsOkWithMediatorResult()
        {
            var expected = new List<PeakUsageTimeDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPeakUsageTimesQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetPeakUsageTimes(null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetDailyTrends_ReturnsOkWithMediatorResult()
        {
            var expected = new List<DailyActivityTrendDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetDailyActivityTrendsQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetDailyTrends(30);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetFailedLogins_ReturnsOkWithMediatorResult()
        {
            var expected = new List<FailedLoginAttemptDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFailedLoginAttemptsQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetFailedLogins(50, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetSecurityAlerts_ReturnsOkWithMediatorResult()
        {
            var expected = new SecurityAlertSummaryDto();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSecurityAlertsSummaryQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetSecurityAlerts(24);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetPerformanceMetrics_ReturnsOkWithMediatorResult()
        {
            var expected = new List<PerformanceMetricsDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPerformanceMetricsQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetPerformanceMetrics(null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task ExportActivities_ReturnsOkWithMediatorResult()
        {
            var expected = new List<ActivityExportDto>();
            _mockMediator.Setup(m => m.Send(It.IsAny<ExportActivitiesQuery>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.ExportActivities(null, null, null, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }
    }
}
