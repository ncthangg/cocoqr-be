namespace CocoQR.Domain.Entities
{
    public class UserToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiredTime { get; set; }
        public virtual User? User { get; set; }
    }
}
