using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Accounts.Requests;
using CocoQR.Application.DTOs.Accounts.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Entities;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
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

        public async Task<PagingVM<GetAccountRes>> GetAllAsync(int pageNumber, int pageSize,
                                                               string? sortField, string? sortDirection,
                                                               Guid? userId,
                                                               Guid? providerId,
                                                               string? searchValue,
                                                               bool? isActive,
                                                               bool? isDeleted,
                                                               bool? status)
        {
            if (!_userContext.IsAdmin() && !_userContext.IsUser())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
            else if (_userContext.IsUser())
            {
                userId = _userContext.UserId;
            }
            else
            {

            }

            var (items, totalCount) = await _unitOfWork.Accounts.GetAllAsync(pageNumber, pageSize,
                                                                             sortField, sortDirection,
                                                                             userId,
                                                                             providerId,
                                                                             searchValue,
                                                                             isActive,
                                                                             isDeleted,
                                                                             status);
            IEnumerable<GetAccountRes> list = [];

            if (_userContext.IsAdmin())
            {
                list = items.Select(p => AccountMapper.ToGetAccountByAdminRes(p)).ToList();
            }
            else if (_userContext.IsUser())
            {
                list = items.Select(p => AccountMapper.ToGetAccountRes(p)).ToList();
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

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

            var account = await _unitOfWork.Accounts.GetByIdAsync(id, _userContext.UserId, _userContext.IsAdmin())
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");


            if (_userContext.IsAdmin())
            {
                return AccountMapper.ToGetAccountByAdminRes(account);
            }
            else if (_userContext.IsUser())
            {
                return AccountMapper.ToGetAccountRes(account);
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }

        public async Task<Guid> PostAccountAsync(PostAccountReq request)
        {
            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            if (request == null)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid request");

            bool exists = await _unitOfWork.Accounts.AccountNumberExistsAsync(
                userId,
                request.AccountNumber,
                request.ProviderId,
                request.BankCode
            );
            if (exists)
                throw new ApplicationException(ErrorCode.DuplicateEntry, "Account number already exists");

            var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Provider not found");

            BankInfo? bank = null;

            if (provider.Code == Domain.Constants.Enum.ProviderCode.BANK)
            {
                if (string.IsNullOrWhiteSpace(request.BankCode))
                    throw new ApplicationException(ErrorCode.ValidationError, "BankCode is required");

                bank = await _unitOfWork.BankInfos.GetByBankCodeAsync(request.BankCode.Trim())
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Bank not found");
            }

            var account = new Account
            {
                UserId = userId,
                AccountNumber = request.AccountNumber.Trim(),
                AccountHolder = request.AccountHolder?.Trim(),
                BankCode = bank?.BankCode,
                Balance = 0,
                ProviderId = request.ProviderId,
                IsActive = request.IsActive,
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
                request.ProviderId,
                request.BankCode,
                id
            );
            if (exists)
                throw new ApplicationException(ErrorCode.DuplicateEntry, "Account number already exists");

            account.AccountNumber = request.AccountNumber.Trim();
            account.AccountHolder = request.AccountHolder?.Trim();
            account.BankCode = request.BankCode?.Trim();
            account.ProviderId = request.ProviderId;
            account.IsPinned = request.IsPinned;
            account.IsActive = request.IsActive;
            account.SetUpdated(userId);

            if (!account.IsValidAccount())
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid account");

            await _unitOfWork.Accounts.UpdateAsync(account);
        }

        public async Task PutStatusAsync(Guid id)
        {
            if (_userContext.IsAdmin())
            {
                if (id == Guid.Empty)
                    throw new ApplicationException(ErrorCode.ValidationError, "Invalid account ID");

                var account = await _unitOfWork.Accounts.GetByIdAsync(id)
                    ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");

                account.ChangeStatus();
                await _unitOfWork.Accounts.UpdateAsync(account);
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
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
