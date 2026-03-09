using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.Request
{
    public class PostPutUserRoleReq
    {
        public required Guid UserId { get; set; }
        public IEnumerable<Guid> RoleIds { get; set; } = [];
    }
}
