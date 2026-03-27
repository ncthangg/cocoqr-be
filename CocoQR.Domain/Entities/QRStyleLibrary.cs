using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Entities
{
    public class QRStyleLibrary
    {
        public QRStyleLibrary() { }
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }

        public string Name { get; set; } = null!;
        public string StyleJson { get; set; } = null!;

        public bool IsDefault { get; set; }

        public QRStyleType Type { get; set; } // System / User
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
