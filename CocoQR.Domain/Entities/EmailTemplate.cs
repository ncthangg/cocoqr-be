namespace CocoQR.Domain.Entities
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public string TemplateKey { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime? UpdatedAt { get; set; }
    }
}
