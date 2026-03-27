namespace CocoQR.Domain.Entities
{
    public class User : BaseEntity
    {
        public User() { }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string GoogleId { get; set; }
        public required string PictureUrl { get; set; }
        public bool Is2FA { get; set; }

        public string? TimeZone { get; set; } // e.g., "Asia/Ho_Chi_Minh", "America/New_York"
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIP { get; set; }
        public string? LastLoginDevice { get; set; }

        public string? SecurityStamp { get; set; }

        public virtual ICollection<UserToken>? UserTokens { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public void UpdateSecurityStamp()
        {
            SecurityStamp = Guid.NewGuid().ToString("N");
        }
    }
}
