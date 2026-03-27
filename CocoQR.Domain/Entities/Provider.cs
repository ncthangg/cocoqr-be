using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Entities
{
    public class Provider : BaseEntity
    {
        public Provider() { }
        public required ProviderCode Code { get; set; }
        public required string Name { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
        public bool IsValidProvider()
        {
            return Enum.IsDefined(typeof(ProviderCode), Code)
                    && !string.IsNullOrWhiteSpace(Name);
        }
    }
}
