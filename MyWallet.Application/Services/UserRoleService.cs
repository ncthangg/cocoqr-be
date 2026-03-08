using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Entities;
using MyWallet.Domain.Helper;
using MyWallet.Domain.Interface.IUnitOfWork;
using System.Collections.Generic;
using System.Reflection.Emit;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;

        public UserRoleService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
        }
        public async Task<PagingVM<GetUserRoleRes>> GetAllUserRoles(int pageNumber, int pageSize, Guid? roleId)
        {
            var (items, totalCount) = await _unitOfWork.UserRoles.GetAllUserRolesAsync(pageNumber,
                                                                      pageSize,
                                                                      roleId);

            var list = items.Select(p => UserRoleMapper.ToGetUserRoleRes(p)).ToList();

            return new PagingVM<GetUserRoleRes>
            {
                List = list,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
        public async Task<IEnumerable<GetRoleRes>> GetRolesByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid user ID");

            var roles = await _unitOfWork.UserRoles.GetRolesByUserIdAsync(userId)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Role of {userId} not found");

            var userDict = await UserHelper.GetUserNameDictAsync<Role>(roles.ToList(), _unitOfWork.Users);

            return roles.Select(p => RoleMapper.ToGetRoleRes(p, userDict)).ToList();
        }
        public async Task<bool> AddUserToRoleAsync(AddUserRoleReq req)
        {
            //var isAdmin = _userContext.IsAdmin();

            //if (!isAdmin)
            //{
            //    throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            //}
            var user = await _unitOfWork.Users.GetByIdAsync(req.UserId);
            if (user == null)
            {
                throw new ApplicationException(ErrorCode.NotFound, "User not found");
            }

            // Check role
            var role = await _unitOfWork.Roles.GetByIdAsync(req.RoleId) 
                ?? throw new ApplicationException(ErrorCode.NotFound, "Role not found");

            // Check duplicate
            var existed = await _unitOfWork.UserRoles.ExistsAsync( req.UserId, req.RoleId);
            if (existed)
            {
                throw new ApplicationException(ErrorCode.BadRequest, "User already has this role");
            }

            var result = await _unitOfWork.UserRoles.AddUserToRoleAsync(_idGenerator.NewId(), req.UserId, req.RoleId);

            return result > 0;
        }
        public async Task<bool> RemoveUserFromRoleAsync(RemoveUserFromRole req)
        {
            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            if (req.UserId == Guid.Empty || req.RoleId == Guid.Empty)
            {
                throw new ApplicationException(ErrorCode.BadRequest, "UserId or RoleId is invalid");
            }

            var affected = await _unitOfWork.UserRoles.RemoveUserFromRoleAsync(req.UserId, req.RoleId);

            if (affected == 0)
                throw new ApplicationException(ErrorCode.NotFound, "User role not found");

            return affected > 0;
        }
    }
}
