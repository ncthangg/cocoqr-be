namespace CocoQR.Application.DTOs.Base.BaseReq
{
    public class BaseReq
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortField { get; set; }
        public string? SortDirection { get; set; }
    }
    public class BaseAdminReq : BaseReq
    {
        public bool? IsDeleted { get; set; }
        public bool? Status { get; set; }
    }
}
