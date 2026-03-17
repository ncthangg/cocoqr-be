using System.ComponentModel.DataAnnotations;

namespace MyWallet.Application.DTOs.QR.Requests
{
    public class PostQrParseReq
    {
        [Required]
        [MinLength(20)]
        public required string Payload { get; set; }
    }
}
