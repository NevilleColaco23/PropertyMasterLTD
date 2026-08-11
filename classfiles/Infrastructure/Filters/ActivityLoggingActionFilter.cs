using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using MyWarehouse.Application.UserActivity.Attributes;
using MyWarehouse.Application.UserActivity.Services;
using MyWarehouse.Domain.UserActivity;
using Messaging.Shared;
using Messaging.Shared.Models;

namespace MyWarehouse.Infrastructure.Filters
{
    /// <summary>
    /// Action filter that automatically logs user activities based on ActivityLog attributes.
    /// Publishes a UserActivityEvent to Azure Service Bus when configured.
    /// Falls back to direct MongoDB write via UserActivityService when Service Bus is not configured.
    /// </summary>
    public class ActivityLoggingActionFilter : IAsyncActionFilter
    {
        private readonly UserActivityService _activityService;
        private readonly ActivityDisplayMessageBuilder _messageBuilder;
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly ServiceBusOptions _serviceBusOptions;

        public ActivityLoggingActionFilter(
            UserActivityService activityService,
            ActivityDisplayMessageBuilder messageBuilder,
            IServiceBusPublisher serviceBusPublisher,
            IOptions<ServiceBusOptions> serviceBusOptions)
        {
            _activityService = activityService;
            _messageBuilder = messageBuilder;
            _serviceBusPublisher = serviceBusPublisher;
            _serviceBusOptions = serviceBusOptions.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Determine whether the action already ran, so we never invoke `next()` more than once
            // (calling it twice would execute the underlying action/command a second time).
            var actionExecuted = false;

            try
            {
                // Check if action has ActivityLog attribute
                var activityLogAttribute = context.ActionDescriptor.EndpointMetadata
                    .OfType<ActivityLogAttribute>()
                    .FirstOrDefault();

                if (activityLogAttribute == null)
                {
                    // No attribute, just execute action
                    actionExecuted = true;
                    await next();
                    return;
                }

                // Start timing
                var stopwatch = Stopwatch.StartNew();

                // Execute the action
                actionExecuted = true;
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
                    var action = BuildAction(activityType, entityType);
                    var ipAddress = GetIpAddress(context);
                    var userAgent = GetUserAgent(context);
                    var sessionId = GetSessionId(context);
                    var isSuccess = IsSuccessResponse(executedContext);
                    var errorMessage = GetErrorMessage(executedContext);
                    var stackTrace = GetStackTrace(executedContext);

                    // ⭐ CRITICAL: Extract selected property ID from header
                    var propertyId = GetSelectedPropertyId(context);

                    // ⭐ NEW: Extract rich metadata from request/response
                    var metadata = ExtractMetadata(context, executedContext);

                    // ⭐ PRIORITY 1: Check if client provided a display message
                    var displayMessage = GetClientProvidedMessage(context);

                    // ⭐ NEW: Determine origin (Client if message provided, otherwise Server)
                    var origin = !string.IsNullOrEmpty(displayMessage) ? "Client" : "Server";

                    // ⭐ PRIORITY 2: If no client message, build one server-side
                    if (string.IsNullOrEmpty(displayMessage))
                    {
                        displayMessage = await _messageBuilder.BuildMessageAsync(
                            activityType,
                            entityType,
                            entityId,
                            username,
                            metadata);
                    }

                    // Publish to Azure Service Bus if configured; otherwise fall back to direct DB write.
                    var isServiceBusConfigured = !string.IsNullOrWhiteSpace(_serviceBusOptions.ConnectionString)
                        && !_serviceBusOptions.ConnectionString.StartsWith("<");

                    if (isServiceBusConfigured)
                    {
                        var activityEvent = new UserActivityEvent
                        {
                            UserId       = userId,
                            Username     = username,
                            ActivityType = (int)activityType,
                            EntityType   = entityType,
                            EntityId     = entityId,
                            Action       = action,
                            DisplayMessage = displayMessage,
                            IPAddress    = ipAddress,
                            Timestamp    = DateTime.UtcNow,
                            Metadata     = metadata?
                                .Where(kv => kv.Value != null)
                                .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty)
                        };

                        await _serviceBusPublisher.SendAsync(
                            activityEvent,
                            _serviceBusOptions.AccessLogQueueName);
                    }
                    else
                    {
                        // Fallback: write directly to MongoDB (local dev without Service Bus)
                        await _activityService.LogActivityAsync(
                            userId: userId,
                            username: username,
                            activityType: activityType,
                            entityType: entityType,
                            entityId: entityId,
                            action: action,
                            module: module,
                            ipAddress: ipAddress,
                            userAgent: userAgent,
                            sessionId: sessionId,
                            isSuccess: isSuccess,
                            errorMessage: errorMessage,
                            stackTrace: stackTrace,
                            durationMs: (int)stopwatch.ElapsedMilliseconds,
                            metadata: metadata,
                            displayMessage: displayMessage,
                            propertyId: propertyId,
                            origin: origin
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the request
                }
            }
            catch (Exception)
            {
                // This catches errors in the filter itself. If the action has not run yet
                // (e.g. an error occurred while inspecting attributes before calling `next()`),
                // execute it now so the request isn't dropped. If it already ran, do NOT call
                // `next()` again - doing so would execute the underlying action a second time
                // (e.g. creating a duplicate post) and can leave the response pipeline in an
                // invalid state, which is what caused the "Post" button to appear broken.
                if (!actionExecuted)
                {
                    await next();
                }
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
            var identity = context.HttpContext.User?.Identity;

            // Try Identity.Name first
            if (!string.IsNullOrEmpty(identity?.Name))
            {
                return identity.Name;
            }

            // Try common JWT claims
            var claims = context.HttpContext.User?.Claims?.ToList();
            if (claims != null && claims.Any())
            {
                // Try different claim types
                var usernameClaim = context.HttpContext.User?.FindFirst("unique_name")?.Value  // ⭐ JWT standard claim (YOUR TOKEN USES THIS!)
                    ?? context.HttpContext.User?.FindFirst("name")?.Value
                    ?? context.HttpContext.User?.FindFirst("username")?.Value
                    ?? context.HttpContext.User?.FindFirst("email")?.Value
                    ?? context.HttpContext.User?.FindFirst("preferred_username")?.Value
                    ?? context.HttpContext.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
                    ?? context.HttpContext.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                if (!string.IsNullOrEmpty(usernameClaim))
                {
                    return usernameClaim;
                }
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
            // First, try to get from route parameters (most common for GetById)
            var routeId = TryGetEntityIdFromRoute(context);
            if (routeId.HasValue)
            {
                return routeId;
            }

            // Then try from response
            if (context.Result is not ObjectResult objectResult)
            {
                return null;
            }

            var result = objectResult.Value;
            if (result == null)
            {
                return null;
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

            return null;
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

        /// <summary>
        /// Extract metadata from request parameters and response data
        /// </summary>
        private Dictionary<string, object> ExtractMetadata(ActionExecutingContext executingContext, ActionExecutedContext executedContext)
        {
            var metadata = new Dictionary<string, object>();

            try
            {
                // Extract from route parameters
                foreach (var routeParam in executingContext.RouteData.Values)
                {
                    if (routeParam.Key != "controller" && routeParam.Key != "action")
                    {
                        metadata[$"route_{routeParam.Key}"] = routeParam.Value?.ToString() ?? "";
                    }
                }

                // Extract from query string
                foreach (var queryParam in executingContext.HttpContext.Request.Query)
                {
                    metadata[$"query_{queryParam.Key}"] = queryParam.Value.ToString();
                }

                // ⭐ Extract selected property from custom header (if present)
                if (executingContext.HttpContext.Request.Headers.TryGetValue("X-Selected-Property", out var selectedPropertyHeader))
                {
                    metadata["selected_PropertyId"] = selectedPropertyHeader.ToString();
                }

                // Extract key info from request body (for POST/PUT)
                foreach (var arg in executingContext.ActionArguments)
                {
                    // Skip large objects, just capture key fields
                    if (arg.Value != null)
                    {
                        var type = arg.Value.GetType();

                        // Capture simple types
                        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime))
                        {
                            metadata[$"param_{arg.Key}"] = arg.Value.ToString();
                        }
                        else
                        {
                            // For complex objects, capture Id/Name fields if they exist
                            var idProp = type.GetProperty("Id");
                            if (idProp != null)
                            {
                                metadata[$"param_{arg.Key}_Id"] = idProp.GetValue(arg.Value)?.ToString() ?? "";
                            }

                            var nameProp = type.GetProperty("Name") ?? type.GetProperty("PropertyName") ?? type.GetProperty("Username");
                            if (nameProp != null)
                            {
                                metadata[$"param_{arg.Key}_Name"] = nameProp.GetValue(arg.Value)?.ToString() ?? "";
                            }
                        }
                    }
                }

                // Extract from response (for GET operations)
                if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
                {
                    var responseType = objectResult.Value.GetType();

                    // For single entities, capture Id and Name
                    var idProp = responseType.GetProperty("Id") ?? responseType.GetProperty("PropertyId");
                    if (idProp != null)
                    {
                        var idValue = idProp.GetValue(objectResult.Value)?.ToString() ?? "";
                        metadata["response_Id"] = idValue;
                    }

                    var nameProp = responseType.GetProperty("Name") ?? responseType.GetProperty("PropertyName") ?? responseType.GetProperty("Title");
                    if (nameProp != null)
                    {
                        var nameValue = nameProp.GetValue(objectResult.Value)?.ToString() ?? "";
                        metadata["response_Name"] = nameValue;
                    }

                    // For list results, capture count
                    var resultsProperty = responseType.GetProperty("Results");
                    if (resultsProperty != null)
                    {
                        var results = resultsProperty.GetValue(objectResult.Value) as System.Collections.IEnumerable;
                        if (results != null)
                        {
                            var count = 0;
                            foreach (var item in results)
                            {
                                count++;
                            }
                            metadata["response_Count"] = count;
                        }
                    }

                    var totalCountProperty = responseType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        var totalValue = totalCountProperty.GetValue(objectResult.Value)?.ToString() ?? "";
                        metadata["response_TotalCount"] = totalValue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't fail if metadata extraction fails
                metadata["metadata_extraction_error"] = ex.Message;
            }

            return metadata;
        }

        /// <summary>
        /// Extracts client-provided activity message from HTTP headers
        /// Clients can send meaningful messages like "Added Ocean View Suite to 3rd floor"
        /// </summary>
        private string? GetClientProvidedMessage(ActionExecutingContext context)
        {
            // Check if client sent X-Activity-Message header
            if (context.HttpContext.Request.Headers.TryGetValue("X-Activity-Message", out var messageHeader))
            {
                var message = messageHeader.ToString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts selected property ID from X-Selected-Property HTTP header
        /// This header is added by the Angular property-context interceptor
        /// </summary>
        private int? GetSelectedPropertyId(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("X-Selected-Property", out var propertyHeader))
            {
                var propertyIdString = propertyHeader.ToString();
                if (!string.IsNullOrWhiteSpace(propertyIdString) && int.TryParse(propertyIdString, out var propertyId))
                {
                    return propertyId;
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

        private string? GetStackTrace(ActionExecutedContext context)
        {
            // PRIORITY 1: Capture exception stack trace if there's an error
            if (context.Exception != null)
            {
                return context.Exception.StackTrace;
            }

            // PRIORITY 2: Capture code execution path (Controller → Service → Repository)
            try
            {
                var stackTrace = new System.Diagnostics.StackTrace(true);
                var frames = stackTrace.GetFrames();

                if (frames != null && frames.Length > 0)
                {
                    // Build simplified execution path
                    var pathBuilder = new System.Text.StringBuilder();
                    var relevantFrames = new List<string>();

                    foreach (var frame in frames)
                    {
                        var method = frame.GetMethod();
                        if (method == null) continue;

                        var declaringType = method.DeclaringType;
                        if (declaringType == null) continue;

                        var typeName = declaringType.Name;
                        var methodName = method.Name;

                        // Only include relevant application frames (skip framework code)
                        if (typeName.EndsWith("Controller") || 
                            typeName.EndsWith("Service") || 
                            typeName.EndsWith("Repository") ||
                            typeName.EndsWith("Handler") ||
                            typeName.EndsWith("Filter"))
                        {
                            relevantFrames.Add($"{typeName}.{methodName}");
                        }
                    }

                    // Build path string: Controller → Service → Repository
                    if (relevantFrames.Any())
                    {
                        return string.Join(" → ", relevantFrames.Distinct().Reverse());
                    }
                }
            }
            catch
            {
                // If we can't capture path, don't fail the request
            }

            return null;
        }
    }
}
