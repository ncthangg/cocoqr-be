namespace CocoQR.Domain.Entities
{
    public class UserRole : BaseEntity
    {
        public required Guid UserId { get; set; }
        public required Guid RoleId { get; set; }

        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
    }
}
