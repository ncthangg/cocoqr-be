using MyWallet.Domain.Constants.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.Providers.Requests
{
    public class PostProviderReq
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        public IFormFile? LogoUrl { get; set; }
    }
    public class PutProviderReq
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        public IFormFile? LogoUrl { get; set; }
        public bool? IsDeleteFile { get; set; }
    }
}
