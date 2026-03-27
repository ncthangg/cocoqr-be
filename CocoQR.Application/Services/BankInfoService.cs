using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Banks.Requests;
using CocoQR.Application.DTOs.Banks.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class BankInfoService : IBankInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IFileStorageService _fileStorageService;

        public BankInfoService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _fileStorageService = fileStorageService;
        }

        public async Task<PagingVM<GetBankInfoRes>> GetsAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue)
        {
            var (items, totalCount) = await _unitOfWork.BankInfos.GetBankInfosAsync(pageNumber,
                                                                      pageSize,
                                                                      sortField,
                                                                      sortDirection,
                                                                      isActive,
                                                                      searchValue,
                                                                      _userContext.IsAdmin());

            var list = items.Select(p => BankInfoMapper.ToGetBankInfoRes(p)).ToList();

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

            return BankInfoMapper.ToGetBankInfoRes(bank);
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

            oldItem.LogoUrl = imageUrl;
            oldItem.IsActive = req.IsActive;
            oldItem.SetUpdated(userId);

            await _unitOfWork.BankInfos.UpdateAsync(oldItem);
        }
    }
}
