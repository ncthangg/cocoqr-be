using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.Users.Responses;

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


            var list = items.Select(p => UserMapper.ToGetUsersRes(p)).ToList();

            return new PagingVM<GetUserBaseRes>
            {
                List = list,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}
