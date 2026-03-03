namespace MyWallet.Application.DTOs.Response
{
    public class GetUserRes
    {
        public required Guid UserId { get; set; }
        public required string GoogleId { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public string SecurityStamp { get; set; } = string.Empty;
    }
}
