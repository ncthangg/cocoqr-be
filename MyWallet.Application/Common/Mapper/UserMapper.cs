using MyWallet.Application.DTOs.Response;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Common.Mapper
{
    public static class UserMapper
    {
        public static GetUserBaseRes ToGetUsersRes(User u, Dictionary<Guid, string>? userDict)
        {
            return new GetUserBaseRes
            {
                Id = u.Id,
                UserId = u.Id,
                Email = u.Email,
                FullName = u.FullName,

                CreatedAt = u.CreatedAt,
                Status = u.Status,
            };
        }
    }
}
