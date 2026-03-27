using CocoQR.Application.DTOs.Roles.Responses;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Common.Mapper
{
    public class RoleMapper
    {
        public static GetRoleRes ToGetRoleRes(Role u)
        {
            return new GetRoleRes
            {
                Id = u.Id,
                Name = u.Name,
                NameUpperCase = u.NameUpperCase,

                Status = u.Status,
                //CreatedAt = u.CreatedAt,
                //UpdatedAt = u.UpdatedAt,
                //DeletedAt = u.DeletedAt,

            };
        }
        public static GetRoleRes ToGetRoleByAdminRes(Role u)
        {
            return new GetRoleRes
            {
                Id = u.Id,
                Name = u.Name,
                NameUpperCase = u.NameUpperCase,

                //Status = u.Status,
                //CreatedAt = u.CreatedAt,
                //UpdatedAt = u.UpdatedAt,
                //DeletedAt = u.DeletedAt,
            };
        }
    }
}
