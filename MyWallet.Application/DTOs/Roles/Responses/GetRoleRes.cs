using MyWallet.Application.DTOs.Base.BaseRes;
using System.Security.Cryptography;

namespace MyWallet.Application.DTOs.Roles.Responses
{
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string NameUpperCase { get; set; }
        public bool? Status { get; set; }
    }
}
