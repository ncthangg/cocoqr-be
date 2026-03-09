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
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;

        public AccountService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
        }

        public async Task<PagingVM<GetAccountRes>> GetUserAccountsAsync(Guid userId, int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue)
        {
            if (userId == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid userId ID");

            var (items, totalCount) = await _unitOfWork.Accounts.GetByUserIdAsync(userId, pageNumber, pageSize, sortField, sortDirection, isActive, searchValue);

            var userDict = await UserHelper.GetUserNameDictAsync((List<BankInfo>)items, _unitOfWork.Users);

            var list = items.Select(p => AccountMapper.ToGetAccountRes(p, userDict)).ToList();

            return new PagingVM<GetAccountRes>
            {
                List = list,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<GetAccountRes> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid userId ID");

            var account = await _unitOfWork.Accounts.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");

            var userDict = await UserHelper.GetUserNameDictAsync(account, _unitOfWork.Users);

            return AccountMapper.ToGetAccountRes(account, userDict);
        }

        public async Task<Guid> PostAccountAsync(PostAccountReq request)
        {
            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            if (request == null)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid request");

            bool exists = await _unitOfWork.Accounts.AccountNumberExistsAsync(
                userId,
                request.AccountNumber
            );
            if (exists)
                throw new ApplicationException(ErrorCode.DuplicateEntry, "Account number already exists");

            var account = new Account
            {
                UserId = userId,
                AccountNumber = request.AccountNumber.Trim(),
                AccountHolder = request.AccountHolder.Trim(),
                BankCode = request.BankCode.Trim(),
                BankName = request.BankName.Trim(),
                AccountType = request.AccountType ?? null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            account.Initialize(_idGenerator.NewId(), userId);

            if (!account.IsValidAccount())
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid account");

            await _unitOfWork.Accounts.AddAsync(account);

            return account.Id;
        }

        public async Task PutAccountAsync(Guid id, PutAccountReq request)
        {
            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid account ID");

            var account = await _unitOfWork.Accounts.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");

            if (account.UserId != userId)
                throw new ApplicationException(ErrorCode.Unauthorized, "Không thuộc quyền sở hữu của user");

            bool exists = await _unitOfWork.Accounts.AccountNumberExistsAsync(
                userId,
                request.AccountNumber,
                id
            );
            if (exists)
                throw new ApplicationException(ErrorCode.DuplicateEntry, "Account number already exists");

            account.AccountNumber = request.AccountNumber.Trim();
            account.AccountHolder = request.AccountHolder.Trim();
            account.BankCode = request.BankCode.Trim();
            account.BankName = request.BankName.Trim();
            account.AccountType = request.AccountType ?? account.AccountType;
            account.SetUpdated(userId);

            if (!account.IsValidAccount())
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid account");

            await _unitOfWork.Accounts.UpdateAsync(account);
        }

        public async Task DeleteAccountAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid account ID");

            _ = await _unitOfWork.Accounts.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");

            await _unitOfWork.Accounts.DeleteAsync(id);
        }
    }
}
