using MongoDB.Bson;
using MongoDB.Bson.IO;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions
{
    /// <summary>
    /// Extension methods for debugging MongoDB queries
    /// Works with any INamedQuery that has a BsonPipeline
    /// </summary>
    public static class MongoQueryDebugExtensions
    {
        /// <summary>
        /// Converts the BsonArray pipeline to a formatted JSON string for MongoDB Compass
        /// Usage: var json = mongoQuery.GetPipelineAsJsonString();
        /// </summary>
        public static string GetPipelineAsJsonString(this INamedQuery query)
        {
            if (query?.BsonPipeline == null)
                return "[]";

            var jsonWriterSettings = new JsonWriterSettings
            {
                Indent = true,
                OutputMode = JsonOutputMode.Shell // Uses MongoDB shell syntax
            };

            return query.BsonPipeline.ToJson(jsonWriterSettings);
        }

        /// <summary>
        /// Gets formatted debug information about the query
        /// Usage: var info = mongoQuery.GetDebugInfo();
        /// </summary>
        public static string GetDebugInfo(this INamedQuery query, string collectionName = "YourCollection")
        {
            if (query == null)
                return "Query is null";

            var queryType = query.GetType().Name;
            var hasPipeline = query.BsonPipeline != null;
            var pipelineStageCount = query.BsonPipeline?.Count ?? 0;

            return $@"
=== MongoDB Query Debug Info ===
Query Type: {queryType}
Collection: {collectionName}
Has Pipeline: {hasPipeline}
Pipeline Stages: {pipelineStageCount}
Query String: {(string.IsNullOrEmpty(query.QueryStr) ? "N/A" : query.QueryStr)}

=== MongoDB Compass Pipeline ===
{(hasPipeline ? query.GetPipelineAsJsonString() : "No pipeline available")}
==========================================
";
        }

        /// <summary>
        /// Gets a simplified debug string with just the query parameters
        /// Useful for quick inspection without the full pipeline
        /// </summary>
        public static string GetDebugSummary(this INamedQuery query)
        {
            if (query == null)
                return "Query is null";

            var queryType = query.GetType().Name;
            var pipelineStageCount = query.BsonPipeline?.Count ?? 0;

            return $"[{queryType}] Pipeline stages: {pipelineStageCount}";
        }
    }
}
