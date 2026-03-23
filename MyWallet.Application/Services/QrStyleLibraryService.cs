using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.QR.Responses;
using MyWallet.Application.DTOs.QRStyleLibrary.Requests;
using MyWallet.Application.DTOs.QRStyleLibrary.Responses;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class QrStyleLibraryService : IQrStyleLibraryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;

        public QrStyleLibraryService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;
        }

        public async Task<IEnumerable<GetQrStyleLibraryRes>> GetAllAsync(QRStyleType? type, bool? isActive)
        {
            Guid? userId = null;
            if (_userContext.IsUser())
            {
                userId = _userContext.UserId;
            }
            var isAdmin = _userContext.IsAdmin();

            var items = await _unitOfWork.QRStyleLibraries.GetAllAsync(userId, type, isActive, isAdmin);

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

        public async Task<Guid> PostUserStyleAsync(PostQRStyleReq request)
        {
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

            return entity.Id;
        }

        public async Task PutUserStyleAsync(Guid styleId, PutQRStyleReq request)
        {
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
            }
            else if (_userContext.IsUser())
            {
                // USER is not allowed to modify IsActive
                style.IsDefault = request.IsDefault ?? style.IsDefault;
            }

            await _unitOfWork.QRStyleLibraries.UpdateAsync(style);
        }

        public async Task DeleteUserStyleAsync(Guid styleId)
        {
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
            var normalized = name.Trim().ToLower();
            var items = await _unitOfWork.QRStyleLibraries.GetAllAsync(userId, type, null, isAdmin);

            var duplicated = items.Any(x =>
                x.Id != excludeId
                && x.Type == type
                && string.Equals(x.Name?.Trim(), normalized, StringComparison.OrdinalIgnoreCase));

            if (duplicated)
                throw new ApplicationException(ErrorCode.Conflict, "Style name already exists");
        }
    }
}
