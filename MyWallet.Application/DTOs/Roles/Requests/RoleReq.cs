namespace MyWallet.Application.DTOs.Roles.Requests
{
    public class PostRoleReq
    {
        public string Name { get; set; } = string.Empty;
    }
    public class PutRoleReq
    {
        public bool Status { get; set; }
    }
}
