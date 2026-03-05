using MyWallet.Application.DTOs.Response;
using MyWallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.Common.Mapper
{
    public class RoleMapper
    {
        public static GetRoleRes ToGetRoleRes(Role u, Dictionary<Guid, string>? userDict)
        {
            return new GetRoleRes
            {
                Id = u.Id,
                Name = u.Name,
                NameUpperCase = u.NameUpperCase,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,

                CreatedByName = BaseMapper.GetUserName(u.CreatedBy, userDict),
                UpdatedByName = BaseMapper.GetUserName(u.UpdatedBy, userDict),
                DeletedByName = BaseMapper.GetUserName(u.DeletedBy, userDict),
            };
        }
        public static GetRoleRes ToGetRoleRes(Role u)
        {
            return new GetRoleRes
            {
                Id = u.Id,
                Name = u.Name,
                NameUpperCase = u.NameUpperCase,
            };
        }
    }
}
