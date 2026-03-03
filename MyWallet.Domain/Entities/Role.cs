using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Domain.Entities
{
    public class Role : BaseEntity
    {
        public required string Name { get; set; }
        public required string NameUpperCase { get; set; }

        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public bool IsValidRole()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(NameUpperCase);
        }
    }
}
