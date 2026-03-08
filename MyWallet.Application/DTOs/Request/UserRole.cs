using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.Request
{
    public class AddUserRoleReq
    {
        public required Guid UserId { get; set; }
        public required Guid RoleId { get; set; }
    }
    public class RemoveUserFromRole
    {
        public required Guid UserId { get; set; }
        public required Guid RoleId { get; set; }
    }
}
