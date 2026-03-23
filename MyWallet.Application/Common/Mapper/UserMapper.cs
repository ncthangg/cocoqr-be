using MyWallet.Application.DTOs.Users.Responses;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Common.Mapper
{
    public static class UserMapper
    {
        public static GetUserBySystemRes ToGetUsersRes(User u)
        {
            return new GetUserBySystemRes
            {
                UserId = u.Id,
                Email = u.Email,
                FullName = u.FullName,

                SecurityStamp = u.SecurityStamp,

                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Status = u.Status,
            };
        }
    }
}
