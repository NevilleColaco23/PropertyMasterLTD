using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Shared.Models
{
    public class AccessLogEvent
    {
        public int SchemaVersion { get; set; } = 1;
        public DateTime TimestampUtc { get; set; }
        public string Method { get; set; } = default!;
        public string Path { get; set; } = default!;
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? TraceId { get; set; }
        public string? ClientIp { get; set; }
        public string? UserAgent { get; set; }
    }
}
