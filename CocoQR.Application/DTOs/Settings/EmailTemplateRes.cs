namespace CocoQR.Application.DTOs.Settings
{
    public sealed class GetEmailTemplateRes
    {
        public Guid Id { get; set; }
        public string TemplateKey { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public List<string> Placeholders { get; set; } = [];
    }
}
