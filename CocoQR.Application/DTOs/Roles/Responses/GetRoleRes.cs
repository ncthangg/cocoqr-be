using CocoQR.Application.DTOs.Base.BaseRes;
using System.Security.Cryptography;

namespace CocoQR.Application.DTOs.Roles.Responses
{
    public class GetRoleRes
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string NameUpperCase { get; set; }
        public bool? Status { get; set; }
    }
}
