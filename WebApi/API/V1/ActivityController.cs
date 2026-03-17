using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Application.UserActivity.Queries;
using MyWarehouse.Domain.UserActivity;
using System;
using System.Threading.Tasks;

namespace MyWarehouse.WebApi.API.V1
{
    /// <summary>
    /// API controller for user activity tracking and queries
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ActivityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get recent activities across all users
        /// </summary>
        /// <param name="count">Number of recent activities to retrieve (default: 50)</param>
        [HttpGet("recent")]
        public async Task<ActionResult<List<UserActivityDTO>>> GetRecentActivities([FromQuery] int count = 50)
        {
            var query = new GetRecentActivitiesQuery(count);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get activities for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="from">Start date (optional)</param>
        /// <param name="to">End date (optional)</param>
        /// <param name="limit">Maximum number of results (optional)</param>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<UserActivityDTO>>> GetUserActivities(
            int userId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int? limit = null)
        {
            var query = new GetUserActivitiesQuery(userId, from, to, limit);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get activities by activity type
        /// </summary>
        /// <param name="activityType">Activity type enum value</param>
        /// <param name="from">Start date (optional)</param>
        /// <param name="to">End date (optional)</param>
        /// <param name="limit">Maximum number of results (optional)</param>
        [HttpGet("type/{activityType}")]
        public async Task<ActionResult<List<UserActivityDTO>>> GetActivitiesByType(
            ActivityType activityType,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int? limit = null)
        {
            var query = new GetActivitiesByTypeQuery(activityType, from, to, limit);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get activity history for a specific entity (audit trail)
        /// </summary>
        /// <param name="entityType">Type of entity (e.g., "Property", "Room")</param>
        /// <param name="entityId">Entity ID</param>
        /// <param name="from">Start date (optional)</param>
        /// <param name="to">End date (optional)</param>
        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<ActionResult<List<UserActivityDTO>>> GetEntityActivities(
            string entityType,
            int entityId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var query = new GetEntityActivitiesQuery(entityType, entityId, from, to);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get paginated activities with filters
        /// </summary>
        /// <param name="userId">Filter by user ID (optional)</param>
        /// <param name="activityType">Filter by activity type (optional)</param>
        /// <param name="entityType">Filter by entity type (optional)</param>
        /// <param name="from">Start date (optional)</param>
        /// <param name="to">End date (optional)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50)</param>
        /// <param name="sortBy">Sort field (default: "Timestamp")</param>
        /// <param name="sortDescending">Sort descending (default: true)</param>
        [HttpGet("paged")]
        public async Task<ActionResult<PagedActivitiesDTO>> GetActivitiesPaged(
            [FromQuery] int? userId = null,
            [FromQuery] ActivityType? activityType = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string sortBy = "Timestamp",
            [FromQuery] bool sortDescending = true)
        {
            var query = new GetActivitiesPagedQuery(
                userId,
                activityType,
                entityType,
                from,
                to,
                page,
                pageSize,
                sortBy,
                sortDescending
            );

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get activity statistics
        /// </summary>
        /// <param name="from">Start date (optional)</param>
        /// <param name="to">End date (optional)</param>
        [HttpGet("statistics")]
        public async Task<ActionResult<ActivityStatisticsDTO>> GetStatistics(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var query = new GetActivityStatisticsQuery(from, to);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get activity summary for dashboard widget
        /// </summary>
        /// <param name="recentCount">Number of recent activities to include (default: 10)</param>
        [HttpGet("summary")]
        public async Task<ActionResult<ActivitySummaryDTO>> GetSummary([FromQuery] int recentCount = 10)
        {
            var query = new GetActivitySummaryQuery(recentCount);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get lightweight activity data for widget display
        /// Returns only DisplayMessage, Timestamp, and Action fields
        /// </summary>
        /// <param name="count">Number of recent activities to retrieve (default: 15)</param>
        [HttpGet("widget")]
        public async Task<ActionResult<List<ActivityWidgetDTO>>> GetWidgetData([FromQuery] int count = 15)
        {
            var query = new GetActivityWidgetDataQuery(count);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
