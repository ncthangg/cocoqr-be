using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Providers.Queries
{
    public class ProviderQueryDto
    {
        public Guid Id { get; set; }
        public required ProviderCode Code { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public string? LogoUrl { get; set; }

        public bool? Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Guid? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public Guid? DeletedBy { get; set; }
        public string? DeletedByName { get; set; }
    }
}
