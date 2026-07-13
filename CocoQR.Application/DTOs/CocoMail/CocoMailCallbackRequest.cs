namespace CocoQR.Application.DTOs.CocoMail
{
    public sealed class CocoMailCallbackRequest
    {
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public Guid EmailId { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string SystemCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public string? ProviderMessageId { get; set; }
        public CocoMailFailureRequest? Failure { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    public sealed class CocoMailFailureRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Retryable { get; set; }
    }
}
