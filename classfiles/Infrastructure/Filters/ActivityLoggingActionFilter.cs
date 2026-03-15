using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyWarehouse.Application.UserActivity.Attributes;
using MyWarehouse.Application.UserActivity.Services;
using MyWarehouse.Domain.UserActivity;

namespace MyWarehouse.Infrastructure.Filters
{
    /// <summary>
    /// Action filter that automatically logs user activities based on ActivityLog attributes
    /// </summary>
    public class ActivityLoggingActionFilter : IAsyncActionFilter
    {
        private readonly UserActivityService _activityService;

        public ActivityLoggingActionFilter(UserActivityService activityService)
        {
            _activityService = activityService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // CRITICAL: Wrap entire filter in try-catch to NEVER crash requests
            try
            {
                // Check if action has ActivityLog attribute
                var activityLogAttribute = context.ActionDescriptor.EndpointMetadata
                    .OfType<ActivityLogAttribute>()
                    .FirstOrDefault();

                if (activityLogAttribute == null)
                {
                    // No attribute, just execute action
                    await next();
                    return;
                }

                // Start timing
                var stopwatch = Stopwatch.StartNew();

                // Execute the action
                var executedContext = await next();

                // Stop timing
                stopwatch.Stop();

                // Determine if we should log (check success status)
                var shouldLog = ShouldLogActivity(executedContext, activityLogAttribute);

                if (!shouldLog)
                {
                    return;
                }

                // Extract activity information (wrapped in try-catch)
                try
                {
                    var userId = GetUserId(context);
                    var username = GetUsername(context);
                    var activityType = ParseActivityType(activityLogAttribute.ActivityType);
                    var entityType = activityLogAttribute.EntityType ?? InferEntityType(context);
                    var entityId = await ExtractEntityId(executedContext, activityLogAttribute);
                    var module = activityLogAttribute.Module ?? InferModule(context);
                    var description = BuildDescription(activityLogAttribute.Description, entityType, entityId, activityType);
                    var action = BuildAction(activityType, entityType);
                    var ipAddress = GetIpAddress(context);
                    var userAgent = GetUserAgent(context);
                    var sessionId = GetSessionId(context);
                    var traceId = GetTraceId(context);
                    var isSuccess = IsSuccessResponse(executedContext);
                    var errorMessage = GetErrorMessage(executedContext);

                    // Log the activity
                    await _activityService.LogActivityAsync(
                        userId: userId,
                        username: username,
                        activityType: activityType,
                        entityType: entityType,
                        entityId: entityId,
                        action: action,
                        description: description,
                        module: module,
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        sessionId: sessionId,
                        traceId: traceId,
                        isSuccess: isSuccess,
                        errorMessage: errorMessage,
                        durationMs: (int)stopwatch.ElapsedMilliseconds
                    );
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the request
                    Console.WriteLine($"❌ Activity Logging Failed: {ex.Message}");
                    Console.WriteLine($"   Stack: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                    }
                }
            }
            catch (Exception outerEx)
            {
                // This catches errors in the filter itself (before action execution)
                Console.WriteLine($"❌ CRITICAL: Activity Filter Error: {outerEx.Message}");
                Console.WriteLine($"   Stack: {outerEx.StackTrace}");

                // Still execute the action even if filter fails
                await next();
            }
        }

        private bool ShouldLogActivity(ActionExecutedContext context, ActivityLogAttribute attribute)
        {
            // If LogOnFailure is true, always log
            if (attribute.LogOnFailure)
            {
                return true;
            }

            // Otherwise, only log if response is successful
            return IsSuccessResponse(context);
        }

        private bool IsSuccessResponse(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                return objectResult.StatusCode >= 200 && objectResult.StatusCode < 300;
            }

            if (context.Result is StatusCodeResult statusCodeResult)
            {
                return statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300;
            }

            // Default to true if we can't determine
            return context.Exception == null;
        }

        private int GetUserId(ActionExecutingContext context)
        {
            var userIdClaim = context.HttpContext.User?.FindFirst("UserId") 
                           ?? context.HttpContext.User?.FindFirst("sub")
                           ?? context.HttpContext.User?.FindFirst("id");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return 0; // Unknown user
        }

        private string GetUsername(ActionExecutingContext context)
        {
            // Try to get username from authenticated user first
            var authenticatedUsername = context.HttpContext.User?.Identity?.Name 
                ?? context.HttpContext.User?.FindFirst("name")?.Value
                ?? context.HttpContext.User?.FindFirst("username")?.Value;

            if (!string.IsNullOrEmpty(authenticatedUsername))
            {
                return authenticatedUsername;
            }

            // For unauthenticated endpoints (like login), try to extract from request body
            if (context.ActionArguments.TryGetValue("login", out var loginObj))
            {
                var usernameProperty = loginObj?.GetType().GetProperty("Username");
                if (usernameProperty != null)
                {
                    var username = usernameProperty.GetValue(loginObj)?.ToString();
                    if (!string.IsNullOrEmpty(username))
                    {
                        return username;
                    }
                }
            }

            // Fallback to Unknown
            return "Unknown";
        }

        private ActivityType ParseActivityType(string activityType)
        {
            if (Enum.TryParse<ActivityType>(activityType, true, out var parsed))
            {
                return parsed;
            }

            // Default to PageView if can't parse
            return ActivityType.PageView;
        }

        private string InferEntityType(ActionExecutingContext context)
        {
            // Get controller name and remove "Controller" suffix
            var controllerName = context.ActionDescriptor.RouteValues["controller"] ?? "Unknown";
            
            // Remove "Controller" suffix if present
            if (controllerName.EndsWith("Controller"))
            {
                controllerName = controllerName.Substring(0, controllerName.Length - 10);
            }

            return controllerName;
        }

        private string InferModule(ActionExecutingContext context)
        {
            // Use controller name as module
            return InferEntityType(context);
        }

        private async Task<int?> ExtractEntityId(ActionExecutedContext context, ActivityLogAttribute attribute)
        {
            if (context.Result is not ObjectResult objectResult)
            {
                return TryGetEntityIdFromRoute(context);
            }

            var result = objectResult.Value;
            if (result == null)
            {
                return TryGetEntityIdFromRoute(context);
            }

            // Try to get ID from specified property
            if (!string.IsNullOrEmpty(attribute.EntityIdProperty))
            {
                var idValue = GetPropertyValue(result, attribute.EntityIdProperty);
                if (idValue != null && int.TryParse(idValue.ToString(), out var id))
                {
                    return id;
                }
            }

            // Try common property names
            var commonIdProperties = new[] { "Id", $"{attribute.EntityType}Id", "EntityId" };
            foreach (var propName in commonIdProperties)
            {
                var idValue = GetPropertyValue(result, propName);
                if (idValue != null && int.TryParse(idValue.ToString(), out var id))
                {
                    return id;
                }
            }

            // Try route parameters
            return TryGetEntityIdFromRoute(context);
        }

        private int? TryGetEntityIdFromRoute(ActionExecutedContext context)
        {
            // Try to get from route data
            if (context.RouteData.Values.TryGetValue("id", out var routeId))
            {
                if (int.TryParse(routeId?.ToString(), out var id))
                {
                    return id;
                }
            }

            return null;
        }

        private object? GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return property?.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }

        private string BuildDescription(string? template, string entityType, int? entityId, ActivityType activityType)
        {
            if (string.IsNullOrEmpty(template))
            {
                // Default description
                return entityId.HasValue
                    ? $"{activityType} {entityType} #{entityId}"
                    : $"{activityType} {entityType}";
            }

            // Replace placeholders
            return template
                .Replace("{entityType}", entityType)
                .Replace("{entityId}", entityId?.ToString() ?? "N/A")
                .Replace("{action}", activityType.ToString());
        }

        private string BuildAction(ActivityType activityType, string entityType)
        {
            return $"{activityType} {entityType}";
        }

        private string? GetIpAddress(ActionExecutingContext context)
        {
            return context.HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent(ActionExecutingContext context)
        {
            return context.HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        }

        private string? GetSessionId(ActionExecutingContext context)
        {
            try
            {
                // Session may not be enabled - use TraceIdentifier as fallback
                return context.HttpContext.Session?.Id;
            }
            catch
            {
                return context.HttpContext.TraceIdentifier;
            }
        }

        private string? GetTraceId(ActionExecutingContext context)
        {
            return context.HttpContext.TraceIdentifier;
        }

        private string? GetErrorMessage(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                return context.Exception.Message;
            }

            if (context.Result is ObjectResult objectResult && objectResult.StatusCode >= 400)
            {
                return objectResult.Value?.ToString();
            }

            return null;
        }
    }
}
