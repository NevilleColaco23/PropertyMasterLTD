using System;

namespace Messaging.Shared.Models
{
    public class EmailQueuedEvent
    {
        public int EmailId { get; set; }
        public string Type { get; set; } = default!;
        public string To { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string BodyHtml { get; set; } = default!;
        public DateTime QueuedAtUtc { get; set; }
    }
}
