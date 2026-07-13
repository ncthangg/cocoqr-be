namespace CocoQR.Domain.Entities
{
    public class CocoMailCallbackEvent
    {
        public Guid Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public Guid EmailId { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
