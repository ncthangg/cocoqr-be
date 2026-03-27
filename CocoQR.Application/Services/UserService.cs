using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Constants;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;

        public UserService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
        }
        public async Task<PagingVM<GetUserBaseRes>> GetUsersAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? status, string? searchValue, Guid? roleId)
        {
            var (items, totalCount) = await _unitOfWork.Users.GetUsersAsync(pageNumber,
                                                                            pageSize,
                                                                            sortField,
                                                                            sortDirection,
                                                                            status,
                                                                            searchValue,
                                                                            roleId);

            return new PagingVM<GetUserBaseRes>
            {
                List = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<GetUserBySystemRes> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid userId ID");

            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");

            return UserMapper.ToGetUsersRes(user);
        }
        public async Task PutStatusAsync(Guid id)
        {
            if (_userContext.IsAdmin())
            {
                if (id == Guid.Empty)
                    throw new ApplicationException(ErrorCode.ValidationError, "Invalid account ID");

                var user = await _unitOfWork.Users.GetByIdAsync(id)
                    ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");

                user.UpdateSecurityStamp();
                user.ChangeStatus();

                var currentUserId = _userContext.UserId
                    ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

                user.SetUpdated(currentUserId);
                await _unitOfWork.Users.UpdateAsync(user);
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }
    }
}
