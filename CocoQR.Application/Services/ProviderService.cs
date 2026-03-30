using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Providers.Requests;
using CocoQR.Application.DTOs.Providers.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        private readonly IFileStorageService _fileStorageService;

        public ProviderService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
            _fileStorageService = fileStorageService;
        }
        public async Task<IEnumerable<GetProviderRes>> GetAllAsync()
        {
            var providers = await _unitOfWork.Providers.GetAllAsync(_userContext.IsAdmin())
            ?? throw new ApplicationException(ErrorCode.NotFound, $"ProviderCode not found");

            var list = providers.Select(p => ProviderMapper.ToGetProviderRes(p)).ToList();
            foreach (var item in list)
            {
                item.LogoUrl = ResolveFileUrl(item.LogoUrl);
            }

            return list;
        }
        public async Task<GetProviderRes> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid userId ID");

            var role = await _unitOfWork.Providers.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"ProviderCode {id} not found");

            var result = ProviderMapper.ToGetProviderRes(role);
            result.LogoUrl = ResolveFileUrl(result.LogoUrl);
            return result;
        }
        public async Task PutAsync(Guid id, PutProviderReq req)
        {
            var isAdmin = _userContext.IsAdmin();

            if (!isAdmin)
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            if (id == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid provider ID");

            Guid userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, "User ID not found in context!");

            var provider = await _unitOfWork.Providers.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"ProviderCode {id} not found");

            if (!Enum.TryParse<ProviderCode>(req.Code, true, out var providerCode))
            {
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid provider code");
            }

            var previousImageUrl = provider.LogoUrl;
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
                imageUrl = await _fileStorageService.UploadFileAsync(req.LogoUrl, $"{FileStorage.Folders.Assets}/{FileStorage.Folders.Providers}");
                hasNewUpload = !string.IsNullOrWhiteSpace(imageUrl) && !string.Equals(imageUrl, previousImageUrl, StringComparison.OrdinalIgnoreCase);
                shouldDeletePreviousAfterDbSuccess = !string.IsNullOrWhiteSpace(previousImageUrl) && hasNewUpload;
            }

            try
            {
                provider.Code = providerCode;
                provider.Name = req.Name;
                provider.IsActive = req.IsActive;
                provider.LogoUrl = imageUrl;
                provider.SetUpdated(userId);

                if (!provider.IsValidProvider())
                    throw new ApplicationException(ErrorCode.ValidationError, "Invalid provider");

                await _unitOfWork.Providers.UpdateAsync(provider);
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

        private string? ResolveFileUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            return _fileStorageService.GetFileUrl(path);
        }
    }
}
