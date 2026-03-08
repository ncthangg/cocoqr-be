using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;
using MyWallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IUserRoleService
    {
        Task<PagingVM<GetUserRoleRes>> GetAllUserRoles(int pageNumber, int pageSize, Guid? roleId);
        Task<IEnumerable<GetRoleRes>> GetRolesByUserIdAsync(Guid userId);
        Task<bool> AddUserToRoleAsync(AddUserRoleReq req);
        Task<bool> RemoveUserFromRoleAsync(RemoveUserFromRole req);
    }
}
