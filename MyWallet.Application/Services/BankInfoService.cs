using Microsoft.IdentityModel.Tokens;
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
using static System.Runtime.InteropServices.JavaScript.JSType;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class BankInfoService : IBankInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        private readonly IFileStorageService _fileStorageService;

        public BankInfoService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
            _fileStorageService = fileStorageService;
        }

        public async Task<PagingVM<GetBankInfoRes>> GetsAsync(int pageNumber, int pageSize, bool? isActive, string? searchValue)
        {
            var (items, totalCount) = await _unitOfWork.BankInfos.GetBankInfosAsync(pageNumber,
                                                                      pageSize,
                                                                      isActive,
                                                                      searchValue);

            var userDict = await UserHelper.GetUserNameDictAsync((List<BankInfo>)items, _unitOfWork.Users);

            var list = items.Select(p => BankInfoMapper.ToGetBankInfoRes(p, userDict)).ToList();

            return new PagingVM<GetBankInfoRes>
            {
                List = list,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
        public async Task<GetBankInfoRes> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid bank ID");

            var bank = await _unitOfWork.BankInfos.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Bank {id} not found");

            var userDict = await UserHelper.GetUserNameDictAsync(bank, _unitOfWork.Users);

            return BankInfoMapper.ToGetBankInfoRes(bank, userDict);
        }
        public async Task PostAsync(PostBankInfoReq req)
        {
            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            string? logoUrl = null;

            if (req.LogoUrl != null)
            {
                logoUrl = await _fileStorageService.UploadFileAsync(req.LogoUrl, $"{FileStorage.Folders.Assets}/{FileStorage.Folders.Banks}");
            }

            var bank = new BankInfo()
            {
                BankCode = req.BankCode,
                NapasCode = req.NapasCode,
                SwiftCode = req.SwiftCode,
                BankName = req.BankName,
                ShortName = req.ShortName,
                LogoUrl = logoUrl ?? null,
                IsActive = req.IsActive,
            };
            bank.Initialize(_idGenerator.NewId(), userId);

            await _unitOfWork.BankInfos.AddAsync(bank);
        }
        public async Task PutAsync(Guid id, PutBankInfoReq req)
        {
            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid bank ID");

            var oldItem = await _unitOfWork.BankInfos.GetByIdAsync(id)
               ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.EntityNotFound);

            string? imageUrl = oldItem.LogoUrl;

            if (req.IsDeleteFile == true)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                    await _fileStorageService.DeleteFileAsync(imageUrl);

                imageUrl = null;
            }
            else if (req.LogoUrl != null)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                    await _fileStorageService.DeleteFileAsync(imageUrl);

                imageUrl = await _fileStorageService.UploadFileAsync(req.LogoUrl, $"{FileStorage.Folders.Assets}/{FileStorage.Folders.Banks}");
            }

            oldItem.BankCode = req.BankCode;
            oldItem.NapasCode = req.NapasCode;
            oldItem.SwiftCode = req.SwiftCode;
            oldItem.BankName = req.BankName;
            oldItem.ShortName = req.ShortName;
            oldItem.LogoUrl = imageUrl;
            oldItem.IsActive = req.IsActive;
            oldItem.SetUpdated(userId);

            await _unitOfWork.BankInfos.UpdateAsync(oldItem);
        }
        public async Task DeleteAsync(Guid id)
        {
            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid bank ID");

            var item = await _unitOfWork.BankInfos.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Bank {id} not found");

            if (!string.IsNullOrEmpty(item.LogoUrl))
                await _fileStorageService.DeleteFileAsync(item.LogoUrl);

            await _unitOfWork.BankInfos.DeleteAsync(id);
        }
    }
}
