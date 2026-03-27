using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Roles.Responses;
using CocoQR.Application.DTOs.UserRoles.Requests;
using CocoQR.Domain.Constants;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
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
        public async Task<IEnumerable<GetRoleRes>> GetRolesByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid user ID");

            var roles = await _unitOfWork.UserRoles.GetRolesByUserIdAsync(userId)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Role of {userId} not found");

            return roles.Select(p => RoleMapper.ToGetRoleRes(p)).ToList();
        }
        public async Task<bool> PostPutUserRolesAsync(PostPutUserRoleReq req)
        {
            if (!_userContext.IsAdmin())
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);

            var user = await _unitOfWork.Users.GetByIdAsync(req.UserId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "User not found");

            var currentRoles = await _unitOfWork.UserRoles.GetRolesByUserIdAsync(req.UserId);

            var currentRoleIds = currentRoles.Select(r => r.Id).ToHashSet();
            var newRoleIds = req.RoleIds.ToHashSet();

            var rolesToAdd = newRoleIds.Except(currentRoleIds);
            var rolesToRemove = currentRoleIds.Except(newRoleIds);

            foreach (var roleId in rolesToAdd)
            {
                await _unitOfWork.UserRoles.AddUserToRoleAsync(
                    _idGenerator.NewId(),
                    req.UserId,
                    roleId
                );
            }

            if (rolesToRemove != null)
                await _unitOfWork.UserRoles.RemoveUserFromRoleAsync(req.UserId, rolesToRemove);

            return true;
        }
    }
}
