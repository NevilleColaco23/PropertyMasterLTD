using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.UserActivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWarehouse.Application.UserActivity.Services
{
    /// <summary>
    /// Centralized service for building human-readable activity display messages
    /// All message formatting logic is centralized here for reusability and maintainability
    /// </summary>
    public class ActivityDisplayMessageBuilder
    {
        private readonly IPropertyRepository _propertyRepository;

        public ActivityDisplayMessageBuilder(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        /// <summary>
        /// Builds a human-readable activity message for reports and widgets
        /// SHORT and concise, with emphasis on important operations (Create/Update/Delete)
        /// </summary>
        /// <param name="activityType">Type of activity</param>
        /// <param name="entityType">Entity type (Property, Room, Booking, etc.)</param>
        /// <param name="entityId">Entity ID if available</param>
        /// <param name="username">Username performing the action</param>
        /// <param name="metadata">Additional context metadata</param>
        /// <returns>Human-readable display message</returns>
        public async Task<string> BuildMessageAsync(
            ActivityType activityType,
            string? entityType,
            int? entityId,
            string username,
            Dictionary<string, object>? metadata = null)
        {
            try
            {
                metadata ??= new Dictionary<string, object>();

                // Determine if this is an important operation (needs more detail)
                var isImportantOperation = IsImportantOperation(activityType);

                // Get user display name (use username if no display name available)
                var userDisplayName = GetUserDisplayName(username, metadata);

                // Get activity verb based on type
                var verb = GetActivityVerb(activityType);

                // Get entity name from metadata or use entity type
                var entityName = GetEntityName(entityType, entityId, metadata, isImportantOperation);

                // Build base message
                var message = $"{userDisplayName} {verb} {entityName}";

                // Only add property context for important operations
                if (isImportantOperation)
                {
                    var propertyContext = await GetPropertyContextAsync(metadata, shortFormat: true);
                    if (!string.IsNullOrEmpty(propertyContext))
                    {
                        message += $" {propertyContext}";
                    }
                }

                // Only add search context for search operations
                if (activityType == ActivityType.Search)
                {
                    var searchContext = GetSearchContext(metadata);
                    if (!string.IsNullOrEmpty(searchContext))
                    {
                        message += $" {searchContext}";
                    }
                }

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to build display message: {ex.Message}");
                // Fallback to basic message
                return $"{username} {GetActivityVerb(activityType)} {entityType ?? "system"}";
            }
        }

        #region Activity Type Classification

        /// <summary>
        /// Determines if an activity type requires detailed logging
        /// </summary>
        public bool IsImportantOperation(ActivityType activityType)
        {
            return activityType switch
            {
                ActivityType.Create => true,
                ActivityType.Update => true,
                ActivityType.Delete => true,
                ActivityType.BulkCreate => true,
                ActivityType.BulkUpdate => true,
                ActivityType.BulkDelete => true,
                ActivityType.StatusChange => true,
                ActivityType.Approve => true,
                ActivityType.Reject => true,
                _ => false // View, PageView, Login, etc. are not "important"
            };
        }

        #endregion

        #region User Display Name

        /// <summary>
        /// Gets user display name from metadata or uses username
        /// </summary>
        private string GetUserDisplayName(string username, Dictionary<string, object> metadata)
        {
            // Check if full name is in metadata
            if (metadata.TryGetValue("userName", out var fullName) && fullName != null)
            {
                return fullName.ToString() ?? username;
            }

            // Use username but make it more readable (remove email domain if present)
            if (username.Contains("@"))
            {
                return username.Split('@')[0];
            }

            return username == "Unknown" ? "A user" : username;
        }

        #endregion

        #region Activity Verbs

        /// <summary>
        /// Gets appropriate verb for activity type
        /// Centralized mapping of ActivityType to human-readable verbs
        /// </summary>
        public string GetActivityVerb(ActivityType activityType)
        {
            return activityType switch
            {
                ActivityType.Create => "created",
                ActivityType.Update => "updated",
                ActivityType.Delete => "deleted",
                ActivityType.View => "viewed",
                ActivityType.PageView => "viewed",
                ActivityType.Search => "searched",
                ActivityType.Export => "exported",
                ActivityType.Import => "imported",
                ActivityType.Login => "logged into",
                ActivityType.Logout => "logged out of",
                ActivityType.Download => "downloaded",
                ActivityType.Upload => "uploaded",
                ActivityType.Approve => "approved",
                ActivityType.Reject => "rejected",
                ActivityType.StatusChange => "changed status of",
                ActivityType.BulkCreate => "bulk created",
                ActivityType.BulkUpdate => "bulk updated",
                ActivityType.BulkDelete => "bulk deleted",
                _ => "interacted with"
            };
        }

        #endregion

        #region Entity Formatting

        /// <summary>
        /// Gets entity name from metadata or formats entity type
        /// </summary>
        private string GetEntityName(string? entityType, int? entityId, Dictionary<string, object> metadata, bool isImportantOperation)
        {
            // For important operations, include entity name and ID
            if (isImportantOperation)
            {
                // Try to get actual name from response metadata
                if (metadata.TryGetValue("response_Name", out var responseName) && responseName != null)
                {
                    var name = responseName.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        return $"'{name}'";
                    }
                }

                // Try to get name from request parameters
                if (metadata.TryGetValue("param_command_Name", out var paramName) && paramName != null)
                {
                    var name = paramName.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        return $"'{name}'";
                    }
                }
            }

            // Format entity type nicely (simplified for views)
            if (!string.IsNullOrEmpty(entityType))
            {
                // For important operations, add ID if available
                if (isImportantOperation && entityId.HasValue)
                {
                    var formatted = FormatEntityType(entityType, isImportantOperation);
                    return $"{formatted} #{entityId}";
                }

                return FormatEntityType(entityType, isImportantOperation);
            }

            return "system";
        }

        /// <summary>
        /// Formats entity type into readable text
        /// SHORT format for views, detailed for important operations
        /// Centralized entity type formatting rules
        /// </summary>
        public string FormatEntityType(string entityType, bool isImportantOperation)
        {
            // For views, use ultra-short format
            if (!isImportantOperation)
            {
                return entityType switch
                {
                    "Dashboards" => "Dashboards",
                    "Properties" => "Properties",
                    "Property" => "Property",
                    "Room" => "Room",
                    "Rooms" => "Rooms",
                    "Booking" => "Booking",
                    "Bookings" => "Bookings",
                    "User" => "User",
                    "Users" => "Users",
                    "Report" => "Report",
                    "Reports" => "Reports",
                    "Dashboard" => "Dashboard",
                    _ => entityType
                };
            }

            // For important operations, use descriptive format
            return entityType switch
            {
                "Dashboards" => "Dashboard",
                "Properties" => "Properties",
                "Property" => "Property",
                "Room" => "Room",
                "Booking" => "Booking",
                "User" => "User",
                "Report" => "Report",
                "Dashboard" => "Dashboard",
                _ => entityType
            };
        }

        #endregion

        #region Property Context

        /// <summary>
        /// Gets property context message if property is selected
        /// SHORT format: "in PropertyName" vs long format: "while working on 'PropertyName' property"
        /// </summary>
        public async Task<string> GetPropertyContextAsync(Dictionary<string, object> metadata, bool shortFormat = false)
        {
            try
            {
                // Check if property ID is in metadata
                if (metadata.TryGetValue("selected_PropertyId", out var propertyIdObj))
                {
                    if (int.TryParse(propertyIdObj?.ToString(), out var propertyId) && propertyId > 0)
                    {
                        // Try to resolve property name
                        var propertyName = await ResolvePropertyNameAsync(propertyId);

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            // Store property name in metadata for future use
                            metadata["selected_PropertyName"] = propertyName;

                            // Return short or long format
                            return shortFormat
                                ? $"in {propertyName}"
                                : $"while working on '{propertyName}' property";
                        }

                        return shortFormat
                            ? $"in Property #{propertyId}"
                            : $"while working on Property #{propertyId}";
                    }
                    else if (propertyId == -1)
                    {
                        // Don't show "all properties" context - it's not useful
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to get property context: {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// Resolves property ID to property name from database
        /// </summary>
        public async Task<string?> ResolvePropertyNameAsync(int propertyId)
        {
            try
            {
                var property = await _propertyRepository.GetByIdAsync(propertyId);
                if (property != null)
                {
                    Console.WriteLine($"✅ Resolved Property #{propertyId} → '{property.Name}'");
                    return property.Name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to resolve property name for ID {propertyId}: {ex.Message}");
            }

            return null;
        }

        #endregion

        #region Search Context

        /// <summary>
        /// Gets search/filter context from metadata (simplified - only search term)
        /// </summary>
        public string GetSearchContext(Dictionary<string, object> metadata)
        {
            // Only show the actual search term, nothing else
            if (metadata.TryGetValue("query_SearchItem", out var searchItem) && searchItem != null)
            {
                var term = searchItem.ToString();
                if (!string.IsNullOrEmpty(term))
                {
                    return $"for '{term}'";
                }
            }

            return string.Empty;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Builds a simple message without async property resolution
        /// Useful for quick logging where property context is not critical
        /// </summary>
        public string BuildSimpleMessage(
            ActivityType activityType,
            string? entityType,
            string username)
        {
            var userDisplayName = GetUserDisplayName(username, new Dictionary<string, object>());
            var verb = GetActivityVerb(activityType);
            var entity = string.IsNullOrEmpty(entityType) ? "system" : entityType;

            return $"{userDisplayName} {verb} {entity}";
        }

        #endregion
    }
}
