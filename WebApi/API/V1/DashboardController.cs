using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.Dashboard.Commands;
using MyWarehouse.Application.Dashboard.DTOs;
using MyWarehouse.Application.Dashboard.Queries;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.WebApi.API.V1
{
    /// <summary>
    /// Dashboard customization API controller
    /// </summary>
    // [Authorize] // TODO: Re-enable after Phase 5 testing
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Get user's dashboard configuration
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="defaultOnly">Get only default dashboard</param>
        /// <returns>Dashboard configuration</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(DashboardConfigurationDTO), 200)]
        [ProducesResponseType(404)]
        [LogView("Dashboard", Description = "User viewed their dashboard configuration")]
        public async Task<ActionResult<DashboardConfigurationDTO>> GetUserDashboard(int userId, [FromQuery] bool defaultOnly = true)
        {
            var result = await _mediator.Send(new GetDashboardByUserIdQuery 
            { 
                UserId = userId,
                DefaultOnly = defaultOnly 
            });

            if (result == null)
                return NotFound(new { message = "Dashboard not found. Please create a dashboard or use a template." });

            return Ok(result);
        }

        /// <summary>
        /// Get all dashboards for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of dashboard configurations</returns>
        [HttpGet("user/{userId}/all")]
        [ProducesResponseType(typeof(List<DashboardConfigurationDTO>), 200)]
        [LogList("Dashboards", Description = "User viewed all their dashboards")]
        public async Task<ActionResult<List<DashboardConfigurationDTO>>> GetAllUserDashboards(int userId)
        {
            var result = await _mediator.Send(new GetUserDashboardsQuery { UserId = userId });
            return Ok(result);
        }

        /// <summary>
        /// Save dashboard configuration (create or update)
        /// </summary>
        /// <param name="command">Dashboard configuration</param>
        /// <returns>Dashboard ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        [ProducesResponseType(400)]
        [LogCreate("Dashboard", EntityIdProperty = "Id")]
        public async Task<ActionResult<string>> SaveDashboard([FromBody] SaveDashboardConfigurationCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                
                if (string.IsNullOrEmpty(command.Id))
                {
                    return CreatedAtAction(nameof(GetUserDashboard), new { userId = command.UserId }, new { id });
                }
                
                return Ok(new { id });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Delete dashboard configuration
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <param name="userId">User ID (for authorization)</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [LogDelete("Dashboard")]
        public async Task<ActionResult> DeleteDashboard(string id, [FromQuery] int userId)
        {
            var result = await _mediator.Send(new DeleteDashboardConfigurationCommand 
            { 
                Id = id,
                UserId = userId 
            });

            if (!result)
                return NotFound(new { message = "Dashboard not found or access denied" });

            return NoContent();
        }

        /// <summary>
        /// Set dashboard as default
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/set-default")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [LogUpdate("Dashboard", Description = "Set dashboard as default")]
        public async Task<ActionResult> SetDefaultDashboard(string id, [FromQuery] int userId)
        {
            var result = await _mediator.Send(new SetDefaultDashboardCommand 
            { 
                DashboardId = id,
                UserId = userId 
            });

            if (!result)
                return NotFound(new { message = "Dashboard not found or access denied" });

            return Ok(new { message = "Dashboard set as default" });
        }

        /// <summary>
        /// Reset dashboard to template
        /// </summary>
        /// <param name="command">Reset command with user ID and template ID</param>
        /// <returns>New dashboard ID</returns>
        [HttpPost("reset-to-template")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        [LogUpdate("Dashboard", Description = "Reset dashboard to template")]
        public async Task<ActionResult<string>> ResetToTemplate([FromBody] ResetDashboardToTemplateCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return Ok(new { id, message = "Dashboard reset to template successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get widget library
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="activeOnly">Return only active widgets</param>
        /// <returns>List of available widgets</returns>
        [HttpGet("widgets")]
        [ProducesResponseType(typeof(List<WidgetLibraryItemDTO>), 200)]
        [LogList("Widgets", Description = "User browsed widget library")]
        public async Task<ActionResult<List<WidgetLibraryItemDTO>>> GetWidgetLibrary(
            [FromQuery] string? category = null,
            [FromQuery] bool activeOnly = true)
        {
            var result = await _mediator.Send(new GetWidgetLibraryQuery 
            { 
                Category = category,
                ActiveOnly = activeOnly 
            });

            return Ok(result);
        }

        /// <summary>
        /// Get dashboard templates
        /// </summary>
        /// <param name="roleId">Optional role ID filter</param>
        /// <param name="publicOnly">Return only public templates</param>
        /// <returns>List of dashboard templates</returns>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(List<DashboardTemplateDTO>), 200)]
        [LogList("Dashboard Templates", Description = "User browsed dashboard templates")]
        public async Task<ActionResult<List<DashboardTemplateDTO>>> GetDashboardTemplates(
            [FromQuery] int? roleId = null,
            [FromQuery] bool publicOnly = true)
        {
            var result = await _mediator.Send(new GetDashboardTemplatesQuery 
            { 
                RoleId = roleId,
                PublicOnly = publicOnly 
            });

            return Ok(result);
        }

        // ==========================================
        // PHASE 4: REAL DATA KPI ENDPOINTS
        // ==========================================

        /// <summary>
        /// Get total properties KPI value
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>KPI value with trend</returns>
        [HttpGet("kpi/total-properties")]
        [ProducesResponseType(typeof(KpiValueResponse), 200)]
        public async Task<ActionResult<KpiValueResponse>> GetTotalPropertiesKpi([FromQuery] int userId)
        {
            var result = await _mediator.Send(new GetTotalPropertiesQuery { UserId = userId });
            return Ok(result);
        }

        /// <summary>
        /// Get total rooms KPI value
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>KPI value</returns>
        [HttpGet("kpi/total-rooms")]
        [ProducesResponseType(typeof(KpiValueResponse), 200)]
        public async Task<ActionResult<KpiValueResponse>> GetTotalRoomsKpi([FromQuery] int userId)
        {
            var result = await _mediator.Send(new GetTotalRoomsQuery { UserId = userId });
            return Ok(result);
        }

        /// <summary>
        /// Get bookings today KPI value
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="propertyIds">Optional property IDs to filter by</param>
        /// <returns>KPI value with trend</returns>
        [HttpGet("kpi/bookings-today")]
        [ProducesResponseType(typeof(KpiValueResponse), 200)]
        public async Task<ActionResult<KpiValueResponse>> GetBookingsTodayKpi(
            [FromQuery] int userId,
            [FromQuery] List<int>? propertyIds = null)
        {
            var result = await _mediator.Send(new GetBookingsTodayQuery 
            { 
                UserId = userId,
                PropertyIds = propertyIds ?? new List<int>()
            });
            return Ok(result);
        }

        /// <summary>
        /// Get occupancy rate KPI value
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>KPI value with trend</returns>
        [HttpGet("kpi/occupancy-rate")]
        [ProducesResponseType(typeof(KpiValueResponse), 200)]
        public async Task<ActionResult<KpiValueResponse>> GetOccupancyRateKpi([FromQuery] int userId)
        {
            var result = await _mediator.Send(new GetOccupancyRateQuery { UserId = userId });
            return Ok(result);
        }

        // ==========================================
        // PHASE 4: ACTIVITY DATA ENDPOINTS
        // ==========================================

        /// <summary>
        /// Get recent activity for dashboard
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of recent activities</returns>
        [HttpGet("activity/recent")]
        [ProducesResponseType(typeof(List<ActivityItemResponse>), 200)]
        public async Task<ActionResult<List<ActivityItemResponse>>> GetRecentActivity(
            [FromQuery] int userId,
            [FromQuery] int limit = 10)
        {
            var result = await _mediator.Send(new GetRecentActivityQuery 
            { 
                UserId = userId,
                Limit = limit 
            });
            return Ok(result);
        }

        /// <summary>
        /// Get recent bookings for dashboard
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of recent bookings</returns>
        [HttpGet("activity/recent-bookings")]
        [ProducesResponseType(typeof(List<RecentBookingResponse>), 200)]
        public async Task<ActionResult<List<RecentBookingResponse>>> GetRecentBookings(
            [FromQuery] int userId,
            [FromQuery] int limit = 10)
        {
            var result = await _mediator.Send(new GetRecentBookingsQuery 
            { 
                UserId = userId,
                Limit = limit 
            });
            return Ok(result);
        }

        /// <summary>
        /// Get calendar events for dashboard
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startDate">Start date for events</param>
        /// <param name="endDate">End date for events</param>
        /// <returns>List of calendar events</returns>
        [HttpGet("activity/calendar-events")]
        [ProducesResponseType(typeof(List<CalendarEventResponse>), 200)]
        public async Task<ActionResult<List<CalendarEventResponse>>> GetCalendarEvents(
            [FromQuery] int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow.Date.AddDays(30);

            var result = await _mediator.Send(new GetCalendarEventsQuery 
            { 
                UserId = userId,
                StartDate = start,
                EndDate = end 
            });
            return Ok(result);
        }

        /// <summary>
        /// Get booking trends for chart widget
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="daysBack">Number of days to look back (default: 30)</param>
        /// <param name="groupBy">Grouping: day, week, or month (default: day)</param>
        /// <returns>Booking trends data</returns>
        [HttpGet("activity/booking-trends")]
        [ProducesResponseType(typeof(BookingTrendsResponse), 200)]
        public async Task<ActionResult<BookingTrendsResponse>> GetBookingTrends(
            [FromQuery] int userId,
            [FromQuery] int daysBack = 30,
            [FromQuery] string groupBy = "day")
        {
            var result = await _mediator.Send(new GetBookingTrendsQuery
            {
                UserId = userId,
                DaysBack = daysBack,
                GroupBy = groupBy
            });
            return Ok(result);
        }
    }
}
