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

                CreatedByName = GetUserName(u.CreatedBy, userDict),
                UpdatedByName = GetUserName(u.UpdatedBy, userDict),
                DeletedByName = GetUserName(u.DeletedBy, userDict),
            };
        }
        private static string? GetUserName(
                Guid? userId,
                IReadOnlyDictionary<Guid, string> dict)
        {
            if (userId == null || userId == Guid.Empty) { return null; }
            return dict.TryGetValue(userId.Value, out var name) ? name : null;
        }
    }
}
