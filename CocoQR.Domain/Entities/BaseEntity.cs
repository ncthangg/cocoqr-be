namespace CocoQR.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool Status { get; set; } = true;

        public void SetId(Guid id)
        {
            if (Id != Guid.Empty)
                throw new InvalidOperationException("Id already set");

            Id = id;
        }
        public void SetCreated(Guid userId)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
        }
        public void SetUpdated(Guid userId)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userId;
        }
        public void SetDeleted(Guid userId)
        {
            DeletedAt = DateTime.UtcNow;
            DeletedBy = userId;
        }
        public virtual void ChangeStatus()
        {
            Status = !Status;
        }

        public void Initialize(Guid id, Guid userId)
        {
            SetId(id);
            SetCreated(userId);
        }
    }
}
