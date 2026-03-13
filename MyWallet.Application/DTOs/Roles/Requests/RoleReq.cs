using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.Roles.Requests
{
    public class PostRoleReq
    {
        public string Name { get; set; } = string.Empty;
    }
    public class PutRoleReq
    {
        public string Name { get; set; } = string.Empty;
    }
}
