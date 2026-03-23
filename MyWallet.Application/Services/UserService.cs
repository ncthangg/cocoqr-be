using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.Users.Responses;
using MyWallet.Domain.Constants;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
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
