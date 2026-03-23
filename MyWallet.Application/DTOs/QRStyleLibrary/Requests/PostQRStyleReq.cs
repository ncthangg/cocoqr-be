using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.QRStyleLibrary.Requests
{
    public class PostQRStyleReq
    {
        public string Name { get; set; } = string.Empty;
        public string StyleJson { get; set; } = string.Empty;

        public bool? IsActive { get; set; }
    }
}
