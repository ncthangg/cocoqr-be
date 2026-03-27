namespace CocoQR.Application.DTOs.Base.BaseRes
{
    public class BaseGetVM<TId>
    {
        public required TId Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public Guid? DeletedBy { get; set; }
        public string? DeletedByName { get; set; }
        public bool? Status { get; set; }
    }
    public class PagingVM<T>
    {
        public IEnumerable<T>? List { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
