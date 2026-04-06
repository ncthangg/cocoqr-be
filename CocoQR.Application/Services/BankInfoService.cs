using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.ICache;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Banks.Requests;
using CocoQR.Application.DTOs.Banks.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Providers.Responses;
using CocoQR.Domain.Constants;
using Microsoft.Extensions.Logging;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class BankInfoService : IBankInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICacheService _cacheService;

        public BankInfoService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, IFileStorageService fileStorageService, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _fileStorageService = fileStorageService;
            _cacheService = cacheService;
        }

        public async Task<PagingVM<GetBankInfoRes>> GetsAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue)
        {
            var isAdmin = _userContext.IsAdmin();
            if (isAdmin)
            {
                var (items, totalCount) = await _unitOfWork.BankInfos.GetBankInfosAsync(pageNumber,
                                                                      pageSize,
                                                                      sortField,
                                                                      sortDirection,
                                                                      isActive,
                                                                      searchValue,
                                                                      _userContext.IsAdmin());

                var list = items.Select(p => BankInfoMapper.ToGetBankInfoRes(p, _fileStorageService)).ToList();

                return new PagingVM<GetBankInfoRes>
                {
                    List = list,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            }
            else
            {
                var cacheKey = "banks:user";

                var cached = await _cacheService.GetAsync<List<GetBankInfoRes>>(cacheKey);

                if (cached == null)
                {
                    var (items, _) = await _unitOfWork.BankInfos.GetBankInfosAsync(
                        1,
                        1000, // lấy hết
                        null,
                        null,
                        null,
                        null,
                        false);

                    cached = items
                        .Select(p => BankInfoMapper.ToGetBankInfoRes(p, _fileStorageService))
                        .ToList();

                    await _cacheService.SetAsync(
                        cacheKey,
                        cached,
                        TimeSpan.FromMinutes(30));
                }

                var query = cached.AsQueryable();

                if (isActive.HasValue)
                    query = query.Where(x => x.IsActive == isActive);

                if (!string.IsNullOrWhiteSpace(searchValue))
                    query = query.Where(x => x.ShortName.Contains(searchValue, StringComparison.OrdinalIgnoreCase));

                var isDesc = sortDirection?.ToUpper() == "DESC";

                query = sortField switch
                {
                    "shortName" => isDesc
                        ? query.OrderByDescending(x => x.ShortName)
                        : query.OrderBy(x => x.ShortName),

                    "bankCode" => isDesc
                        ? query.OrderByDescending(x => x.BankCode)
                        : query.OrderBy(x => x.BankCode),

                    _ => query.OrderBy(x => x.ShortName)
                };

                var totalCount = query.Count();

                var result = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagingVM<GetBankInfoRes>
                {
                    List = result,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            }
            //    var (items, totalCount) = await _unitOfWork.BankInfos.GetBankInfosAsync(pageNumber,
            //                                                          pageSize,
            //                                                          sortField,
            //                                                          sortDirection,
            //                                                          isActive,
            //                                                          searchValue,
            //                                                          _userContext.IsAdmin());

            //var list = items.Select(p => BankInfoMapper.ToGetBankInfoRes(p, _fileStorageService)).ToList();

            //return new PagingVM<GetBankInfoRes>
            //{
            //    List = list,
            //    PageNumber = pageNumber,
            //    PageSize = pageSize,
            //    TotalItems = totalCount,
            //    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            //};
        }
        public async Task<GetBankInfoRes> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid bank ID", nameof(id));

            var bank = await _unitOfWork.BankInfos.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Bank {id} not found");

            return BankInfoMapper.ToGetBankInfoRes(bank, _fileStorageService);
        }
        public async Task PutAsync(Guid id, PutBankInfoReq req)
        {
            ArgumentNullException.ThrowIfNull(req);

            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

            if (id == Guid.Empty)
                throw new ArgumentException("Invalid bank ID", nameof(id));

            var oldItem = await _unitOfWork.BankInfos.GetByIdAsync(id)
               ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.EntityNotFound);

            var previousImageUrl = oldItem.LogoUrl;
            var imageUrl = previousImageUrl;
            var hasNewUpload = false;
            var shouldDeletePreviousAfterDbSuccess = false;

            if (req.IsDeleteFile == true)
            {
                imageUrl = null;
                shouldDeletePreviousAfterDbSuccess = !string.IsNullOrWhiteSpace(previousImageUrl);
            }
            else if (req.LogoUrl != null)
            {
                imageUrl = await _fileStorageService.UploadFileAsync(req.LogoUrl, $"{FileStorage.Folders.Assets}/{FileStorage.Folders.Banks}");
                hasNewUpload = !string.IsNullOrWhiteSpace(imageUrl) && !string.Equals(imageUrl, previousImageUrl, StringComparison.OrdinalIgnoreCase);
                shouldDeletePreviousAfterDbSuccess = !string.IsNullOrWhiteSpace(previousImageUrl) && hasNewUpload;
            }

            try
            {
                oldItem.LogoUrl = imageUrl;
                oldItem.IsActive = req.IsActive;
                oldItem.SetUpdated(userId);

                await _unitOfWork.BankInfos.UpdateAsync(oldItem);
            }
            catch
            {
                if (hasNewUpload && !string.IsNullOrWhiteSpace(imageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(imageUrl);
                }

                throw;
            }

            if (shouldDeletePreviousAfterDbSuccess && !string.IsNullOrWhiteSpace(previousImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(previousImageUrl);
            }
        }
    }
}
