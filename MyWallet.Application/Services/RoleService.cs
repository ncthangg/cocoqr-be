using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Application.DTOs.Roles.Requests;
using MyWallet.Application.DTOs.Roles.Responses;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Constants.Enum;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        public RoleService(IUnitOfWork unitOfWork, IUserContext userContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
        }
        public async Task<IEnumerable<GetRoleRes>> GetAllAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync()
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Roles not found");


            return roles.Select(p => RoleMapper.ToGetRoleRes(p)).ToList();
        }
        public async Task PutAsync(Guid id, PutRoleReq req)
        {
            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid role ID");

            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            var role = await _unitOfWork.Roles.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Role {id} not found");

            if (role.Name.Trim().ToLower() == RoleCategory.ADMIN.ToString().ToLower() && req.Status == false)
                throw new ApplicationException(ErrorCode.ValidationError, "Admin role cannot be deactivated");

            role.Status = req.Status;
            role.SetUpdated(userId);

            await _unitOfWork.Roles.UpdateAsync(role);
        }
    }
}
