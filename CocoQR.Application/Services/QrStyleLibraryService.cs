using CocoQR.Application.Contracts.ICache;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.QRStyleLibrary.Requests;
using CocoQR.Application.DTOs.QRStyleLibrary.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;
using DomainException = CocoQR.Domain.Exceptions.DomainException;

namespace CocoQR.Application.Services
{
    public class QrStyleLibraryService : IQrStyleLibraryService
    {
        private static readonly TimeSpan QrStyleCacheExpiry = TimeSpan.FromMinutes(5);
        private const string SystemStylesCacheKey = "qr-style:system";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        private readonly ICacheService _cacheService;

        public QrStyleLibraryService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<GetQrStyleLibraryRes>> GetAllAsync(QRStyleType? type, bool? isActive)
        {
            if (!_userContext.IsAuthenticated())
            {
                if (type == QRStyleType.USER)
                    throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);

                var publicResult = await GetSystemStylesAsync(false);

                return publicResult.ToList();
            }

            IEnumerable<GetQrStyleLibraryRes> result;
            if (_userContext.IsAdmin())
            {
                result = await GetSystemStylesAsync(_userContext.IsAdmin());
            }
            else if (_userContext.IsUser())
            {
                var userId = _userContext.UserId
                    ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

                result = type switch
                {
                    QRStyleType.SYSTEM => (await GetAllUserAvailableStyles(userId)).Where(x => x.Type == QRStyleType.SYSTEM),
                    QRStyleType.USER => await GetUserStylesAsync(userId),
                    _ => await GetAllUserAvailableStyles(userId)
                };
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            if (isActive.HasValue)
            {
                result = result.Where(x => x.IsActive == isActive.Value);
            }

            return result.ToList();
        }

        protected async Task<IEnumerable<GetQrStyleLibraryRes>> GetSystemStylesAsync(bool isAdmin)
        {
            var cached = await _cacheService.GetAsync<List<GetQrStyleLibraryRes>>(SystemStylesCacheKey);
            if (cached is not null)
            {
                return cached;
            }

            var items = await _unitOfWork.QRStyleLibraries.GetAllAsync(null, QRStyleType.SYSTEM, null, isAdmin);
            var mapped = MapToResponse(items);

            await _cacheService.SetAsync(SystemStylesCacheKey, mapped, QrStyleCacheExpiry);
            return mapped;
        }

        protected async Task<IEnumerable<GetQrStyleLibraryRes>> GetUserStylesAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            var cacheKey = BuildUserStylesCacheKey(userId);
            var cached = await _cacheService.GetAsync<List<GetQrStyleLibraryRes>>(cacheKey);
            if (cached is not null)
            {
                return cached;
            }

            var items = await _unitOfWork.QRStyleLibraries.GetAllAsync(userId, QRStyleType.USER, null, false);
            var mapped = MapToResponse(items).Where(x => x.Type == QRStyleType.USER).ToList();

            await _cacheService.SetAsync(cacheKey, mapped, QrStyleCacheExpiry);
            return mapped;
        }

        protected async Task<IEnumerable<GetQrStyleLibraryRes>> GetAllUserAvailableStyles(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            var cacheKey = BuildAllUserAvailableStylesCacheKey(userId);
            var cached = await _cacheService.GetAsync<List<GetQrStyleLibraryRes>>(cacheKey);
            if (cached is not null)
            {
                return cached;
            }

            var items = await _unitOfWork.QRStyleLibraries.GetAllAsync(userId, null, null, false);
            var mapped = MapToResponse(items);

            await _cacheService.SetAsync(cacheKey, mapped, QrStyleCacheExpiry);
            return mapped;
        }

        public async Task<Guid> PostUserStyleAsync(PostQRStyleReq request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!_userContext.IsAuthenticated())
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);

            var (userId, type, isAdmin) = ResolveWriteScope();

            await EnsureNameNotDuplicatedAsync(request.Name, userId, type, isAdmin, null);

            var entity = new QRStyleLibrary
            {
                Id = _idGenerator.NewId(),
                UserId = userId,
                Name = request.Name,
                StyleJson = request.StyleJson,
                IsDefault = false,
                Type = type,
                IsActive = request.IsActive ?? false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.QRStyleLibraries.AddAsync(entity);

            await InvalidateCacheAfterWriteAsync(entity.Type, entity.UserId);

            return entity.Id;
        }

        public async Task PutUserStyleAsync(Guid styleId, PutQRStyleReq request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (styleId == Guid.Empty)
                throw new ArgumentException("Invalid style ID", nameof(styleId));

            if (!_userContext.IsAuthenticated())
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);

            var style = await _unitOfWork.QRStyleLibraries.GetByIdAsync(styleId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Style not found");

            if (_userContext.IsAdmin())
            {
                if (style.Type != QRStyleType.SYSTEM)
                    throw new ApplicationException(ErrorCode.Forbidden, "Admin can modify only system styles");
            }
            else if (_userContext.IsUser())
            {
                var currentUserId = _userContext.UserId
                    ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

                if (style.Type != QRStyleType.USER || style.UserId != currentUserId)
                    throw new ApplicationException(ErrorCode.Forbidden, "User can modify only their own USER styles");
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            await EnsureNameNotDuplicatedAsync(request.Name, style.UserId, style.Type, _userContext.IsAdmin(), style.Id);

            style.Name = request.Name;
            style.StyleJson = request.StyleJson;

            if (_userContext.IsAdmin())
            {
                // ADMIN is not allowed to modify IsDefault
                if (request.IsActive.HasValue)
                {
                    style.IsActive = request.IsActive.Value;
                }
                if (request.IsDefault.HasValue)
                {
                    style.IsDefault = request.IsDefault.Value;
                }
            }
            else if (_userContext.IsUser())
            {
                // USER is not allowed to modify IsActive
                style.IsDefault = request.IsDefault ?? style.IsDefault;
            }

            await _unitOfWork.QRStyleLibraries.UpdateAsync(style);

            await InvalidateCacheAfterWriteAsync(style.Type, style.UserId);
        }

        public async Task DeleteUserStyleAsync(Guid styleId)
        {
            if (styleId == Guid.Empty)
                throw new ArgumentException("Invalid style ID", nameof(styleId));

            if (!_userContext.IsAuthenticated())
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);

            var style = await _unitOfWork.QRStyleLibraries.GetByIdAsync(styleId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Style not found");

            if (_userContext.IsAdmin())
            {
                if (style.Type != QRStyleType.SYSTEM)
                    throw new ApplicationException(ErrorCode.Forbidden, "Admin can delete only system styles");
            }
            else if (_userContext.IsUser())
            {
                var currentUserId = _userContext.UserId
                    ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

                if (style.Type != QRStyleType.USER || style.UserId != currentUserId)
                    throw new ApplicationException(ErrorCode.Forbidden, "User can delete only their own USER styles");
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            await _unitOfWork.QRStyleLibraries.DeleteAsync(styleId);

            await InvalidateCacheAfterWriteAsync(style.Type, style.UserId);
        }

        private (Guid? userId, QRStyleType type, bool isAdmin) ResolveWriteScope()
        {
            if (_userContext.IsAdmin())
            {
                return (null, QRStyleType.SYSTEM, true);
            }

            if (_userContext.IsUser())
            {
                var userId = _userContext.UserId
                    ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);
                return (userId, QRStyleType.USER, false);
            }

            throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
        }

        private async Task EnsureNameNotDuplicatedAsync(string name, Guid? userId, QRStyleType type, bool isAdmin, Guid? excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Style name is required", nameof(name));

            var normalized = name.Trim().ToLower();
            var items = await _unitOfWork.QRStyleLibraries.GetAllAsync(userId, type, null, isAdmin);

            var duplicated = items.Any(x =>
                x.Id != excludeId
                && x.Type == type
                && string.Equals(x.Name?.Trim(), normalized, StringComparison.OrdinalIgnoreCase));

            if (duplicated)
                throw new DomainException(ErrorCode.DuplicateEntry, "Style name already exists");
        }

        private static List<GetQrStyleLibraryRes> MapToResponse(IEnumerable<QRStyleLibrary> items)
        {
            return items.Select(p => new GetQrStyleLibraryRes
            {
                Id = p.Id,
                Name = p.Name,
                StyleJson = p.StyleJson,
                IsDefault = p.IsDefault,
                Type = p.Type,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        private static string BuildUserStylesCacheKey(Guid userId) => $"qr-style:user:{userId}";

        private static string BuildAllUserAvailableStylesCacheKey(Guid userId) => $"qr-style:all:{userId}";

        private async Task InvalidateCacheAfterWriteAsync(QRStyleType type, Guid? userId)
        {
            if (type == QRStyleType.SYSTEM)
            {
                await _cacheService.RemoveAsync(SystemStylesCacheKey);
                return;
            }

            if (type == QRStyleType.USER && userId.HasValue)
            {
                await _cacheService.RemoveAsync(BuildUserStylesCacheKey(userId.Value));
                await _cacheService.RemoveAsync(BuildAllUserAvailableStylesCacheKey(userId.Value));
            }
        }
    }
}
