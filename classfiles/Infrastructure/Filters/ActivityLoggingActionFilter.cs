using System;
using System.Collections.Generic;
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
        private readonly ActivityDisplayMessageBuilder _messageBuilder;

        public ActivityLoggingActionFilter(
            UserActivityService activityService,
            ActivityDisplayMessageBuilder messageBuilder)
        {
            _activityService = activityService;
            _messageBuilder = messageBuilder;
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

                    // ⭐ CRITICAL: Extract selected property ID from header
                    var propertyId = GetSelectedPropertyId(context);
                    if (propertyId.HasValue)
                    {
                        Console.WriteLine($"🏠 Selected Property ID: {propertyId.Value}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No property context in request");
                    }

                    // ⭐ NEW: Extract rich metadata from request/response
                    var metadata = ExtractMetadata(context, executedContext);

                    // ⭐ PRIORITY 1: Check if client provided a display message
                    var displayMessage = GetClientProvidedMessage(context);

                    // ⭐ PRIORITY 2: If no client message, build one server-side
                    if (string.IsNullOrEmpty(displayMessage))
                    {
                        displayMessage = await _messageBuilder.BuildMessageAsync(
                            activityType,
                            entityType,
                            entityId,
                            username,
                            metadata);
                        Console.WriteLine($"🤖 Auto-generated message: {displayMessage}");
                    }
                    else
                    {
                        Console.WriteLine($"📱 Client-provided message: {displayMessage}");
                    }

                    // Log the activity with metadata and display message
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
                        durationMs: (int)stopwatch.ElapsedMilliseconds,
                        metadata: metadata,  // ⭐ Technical metadata for querying
                        displayMessage: displayMessage,  // ⭐ Human-readable message for reports
                        propertyId: propertyId  // ⭐ CRITICAL: Pass property context!
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
            Console.WriteLine("🔍 GetUsername - Starting username resolution");

            // Try to get username from authenticated user first
            var identity = context.HttpContext.User?.Identity;
            if (identity != null)
            {
                Console.WriteLine($"   Identity.IsAuthenticated: {identity.IsAuthenticated}");
                Console.WriteLine($"   Identity.Name: {identity.Name}");
            }

            // Try Identity.Name first
            if (!string.IsNullOrEmpty(identity?.Name))
            {
                Console.WriteLine($"✅ Found username from Identity.Name: {identity.Name}");
                return identity.Name;
            }

            // Try common JWT claims
            var claims = context.HttpContext.User?.Claims?.ToList();
            if (claims != null && claims.Any())
            {
                Console.WriteLine($"   Total claims found: {claims.Count}");
                foreach (var claim in claims.Take(10))  // Log first 10 claims for debugging
                {
                    Console.WriteLine($"   Claim: {claim.Type} = {claim.Value}");
                }

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
                    Console.WriteLine($"✅ Found username from claims: {usernameClaim}");
                    return usernameClaim;
                }
            }
            else
            {
                Console.WriteLine("   ⚠️ No claims found");
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
                        Console.WriteLine($"✅ Found username from login request: {username}");
                        return username;
                    }
                }
            }

            // Fallback to Unknown
            Console.WriteLine("❌ Username resolution failed - returning 'Unknown'");
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
                Console.WriteLine($"🔍 ExtractMetadata START");
                Console.WriteLine($"   Route values count: {executingContext.RouteData.Values.Count}");
                Console.WriteLine($"   Query params count: {executingContext.HttpContext.Request.Query.Count}");
                Console.WriteLine($"   Action arguments count: {executingContext.ActionArguments.Count}");

                // Extract from route parameters
                foreach (var routeParam in executingContext.RouteData.Values)
                {
                    if (routeParam.Key != "controller" && routeParam.Key != "action")
                    {
                        metadata[$"route_{routeParam.Key}"] = routeParam.Value?.ToString() ?? "";
                        Console.WriteLine($"   ✅ Route param: {routeParam.Key} = {routeParam.Value}");
                    }
                }

                // Extract from query string
                foreach (var queryParam in executingContext.HttpContext.Request.Query)
                {
                    metadata[$"query_{queryParam.Key}"] = queryParam.Value.ToString();
                    Console.WriteLine($"   ✅ Query param: {queryParam.Key} = {queryParam.Value}");
                }

                // ⭐ Extract selected property from custom header (if present)
                if (executingContext.HttpContext.Request.Headers.TryGetValue("X-Selected-Property", out var selectedPropertyHeader))
                {
                    metadata["selected_PropertyId"] = selectedPropertyHeader.ToString();
                    Console.WriteLine($"   🏠 Selected Property: {selectedPropertyHeader}");
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
                    Console.WriteLine($"   📦 Response type: {responseType.Name}");

                    // For single entities, capture Id and Name
                    var idProp = responseType.GetProperty("Id") ?? responseType.GetProperty("PropertyId");
                    if (idProp != null)
                    {
                        var idValue = idProp.GetValue(objectResult.Value)?.ToString() ?? "";
                        metadata["response_Id"] = idValue;
                        Console.WriteLine($"   ✅ Response Id: {idValue}");
                    }

                    var nameProp = responseType.GetProperty("Name") ?? responseType.GetProperty("PropertyName") ?? responseType.GetProperty("Title");
                    if (nameProp != null)
                    {
                        var nameValue = nameProp.GetValue(objectResult.Value)?.ToString() ?? "";
                        metadata["response_Name"] = nameValue;
                        Console.WriteLine($"   ✅ Response Name: {nameValue}");
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
                            Console.WriteLine($"   ✅ Response Count: {count}");
                        }
                    }

                    var totalCountProperty = responseType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        var totalValue = totalCountProperty.GetValue(objectResult.Value)?.ToString() ?? "";
                        metadata["response_TotalCount"] = totalValue;
                        Console.WriteLine($"   ✅ Response TotalCount: {totalValue}");
                    }
                }
                else
                {
                    Console.WriteLine($"   ⚠️ No ObjectResult or Value is null");
                }

                Console.WriteLine($"🔍 ExtractMetadata END - Total metadata items: {metadata.Count}");
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
