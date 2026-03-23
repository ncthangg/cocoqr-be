using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.QR.Requests;
using MyWallet.Application.DTOs.QR.Responses;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;
using MyWallet.QR_Generator.Interface;
using MyWallet.QR_Generator.Models;
using System.ComponentModel.DataAnnotations;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class QrService : IQrService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        private readonly IQrPayloadEngine _qrEngine;

        public QrService(IUnitOfWork unitOfWork, IUserContext userContext,
                    IQrPayloadEngine qrEngine,
                    IQrImageRenderer qrImageRenderer)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;

            _qrEngine = qrEngine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortField"></param>
        /// <param name="sortDirection"></param>
        /// <param name="userId"></param>
        /// <param name="providerId"></param>
        /// <param name="searchValue"> AccountNumber/ AccountHolder/ BankCode/ NapasBin </param>
        /// <param name="isDeleted"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<PagingVM<GetQrRes>> GetAllAsync(int pageNumber, int pageSize,
                                                          string? sortField, string? sortDirection,
                                                          Guid? userId,
                                                          Guid? providerId,
                                                          string? searchValue,
                                                          bool? isDeleted,
                                                          bool? status)
        {
            if (userId == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid userId ID");

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

            var (items, totalCount) = await _unitOfWork.QRHistories.GetAllAsync(pageNumber, pageSize,
                                                                                sortField, sortDirection,
                                                                                userId,
                                                                                providerId,
                                                                                searchValue,
                                                                                isDeleted,
                                                                                status);
            IEnumerable<GetQrRes> list = [];

            if (_userContext.IsAdmin())
            {
                list = items.Select(p => QrMapper.ToGetQrHistoryByAdminRes(p)).ToList();
            }
            else if (_userContext.IsUser())
            {
                list = items.Select(p => QrMapper.ToGetQrHistoryRes(p)).ToList();
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }

            return new PagingVM<GetQrRes>
            {
                List = list,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<GetQrRes> GetByIdAsync(long id)
        {
            if (id == null)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid qr ID");

            var account = await _unitOfWork.QRHistories.GetByIdAsync(id, _userContext.UserId, _userContext.IsAdmin())
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Account {id} not found");


            if (_userContext.IsAdmin())
            {
                return QrMapper.ToGetQrHistoryByAdminRes(account);
            }
            else if (_userContext.IsUser())
            {
                return QrMapper.ToGetQrHistoryRes(account);
            }
            else
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }
        public async Task<PostQrRes> GenerateAsync(PostQrReq request)
        {
            // 1. Resolve provider để xác định Mode
            var provider = await GetProviderAsync(request.ProviderId);
            var mode = ResolveMode(provider.Code, request.QrMode);

            // 2. Resolve account info
            var (accountNumber, accountHolder, bankCode, bankName, bankShortName, napasBin)
                = await ResolveAccountAsync(request, mode);

            // 3. Generate EMVCo payload qua engine
            var engineRequest = new QrGenerateRequest
            {
                ProviderCode = provider.Code,
                BankCode = bankCode,
                AccountNumber = accountNumber,
                Amount = request.Amount,
                Description = request.Description,
                IsStatic = request.Amount == null,
                Mode = mode,
            };

            var payloadResult = _qrEngine.Generate(engineRequest);

            // 4. Persist — chỉ lưu payload string, KHÔNG lưu ảnh vào DB
            var entity = new QRHistory
            {
                UserId = _userContext.UserId ?? null,
                AccountId = request.AccountId ?? null,
                AccountNumberSnapshot = accountNumber,
                AccountHolderSnapshot = accountHolder,

                BankCodeSnapshot = bankCode ?? null,
                BankNameSnapshot = bankName ?? null,
                BankShortNameSnapshot = bankShortName ?? null,
                NapasBinSnapshot = payloadResult.NapasBin ?? null,

                Amount = request.Amount,
                Description = request.Description,

                QrData = payloadResult.Payload,
                TransactionRef = GenerateTransactionRef(),

                ReceiverType = request.AccountId.HasValue ? QRReceiverType.PERSONAL : QRReceiverType.GUEST,
                ProviderId = provider.Id,
                IsFixedAmount = request.IsFixedAmount,
                QrMode = mode,

                CreatedAt = DateTime.UtcNow,
            };

            var id = await _unitOfWork.QRHistories.Post(entity);

            await SaveQrStyleSnapshotAsync(id, request);
            var style = await _unitOfWork.QRStyles.GetByQrIdAsync(id);

            // 6. Return — ảnh generate on-the-fly, không lưu DB
            return new PostQrRes
            {
                Id = id,
                QrData = payloadResult.Payload,
                StyleJson = style?.StyleJson,
                TransactionRef = entity.TransactionRef,
                IsValid = payloadResult.IsValid,
            };
        }

        public async Task<PostQrRes> RegenerateImageAsync(Guid qrHistoryId)
        {
            // Lấy payload từ DB → render lại ảnh — không cần lưu ảnh
            var history = await _unitOfWork.QRHistories.GetByIdAsync(qrHistoryId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "QR history không tồn tại.");

            var style = await _unitOfWork.QRStyles.GetByQrIdAsync(history.Id);

            return new PostQrRes
            {
                Id = history.Id,
                QrData = history.QrData,
                StyleJson = style?.StyleJson,
                TransactionRef = history.TransactionRef,
                IsValid = _qrEngine.Verify(history.QrData),
            };
        }

        #region
        private static string GenerateTransactionRef()
             => $"COCO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        private static QrMode ResolveMode(ProviderCode providerCode, QrMode? requestedMode)
        {
            if (providerCode == ProviderCode.MOMO && requestedMode == QrMode.MomoNative)
                return QrMode.MomoNative;

            return QrMode.VietQR;
        }

        private async Task SaveQrStyleSnapshotAsync(long qrId, PostQrReq request)
        {
            if (request.UseDefault)
                return;

            if (string.IsNullOrWhiteSpace(request.StyleJson))
                return;

            if (request.StyleId.HasValue)
            {
                var styleLib = await _unitOfWork.QRStyleLibraries.GetByIdAsync(request.StyleId.Value)
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Style library not found");

                if (!styleLib.IsActive)
                    throw new ApplicationException(ErrorCode.BadRequest, "Style library is inactive");

                if (styleLib.Type == QRStyleType.USER)
                {
                    var currentUserId = _userContext.UserId
                        ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);

                    if (styleLib.UserId != currentUserId)
                        throw new ApplicationException(ErrorCode.Forbidden, "No permission for selected style");
                }
            }

            var style = new QRStyle
            {
                Id = Guid.NewGuid(),
                QrId = qrId,
                StyleJson = request.StyleJson,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.QRStyles.AddAsync(style);
        }

        private async Task<(string accountNumber,
                            string? holder,
                            string? bankCode,
                            string? bankName,
                            string? bankShortName,
                            string? napasBin)>
        ResolveAccountAsync(PostQrReq request, QrMode mode)
        {
            var userId = _userContext.UserId;

            // =========================
            // CASE 1: DÙNG ACCOUNT ĐÃ LƯU
            // =========================
            if (request.AccountId.HasValue)
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId.Value)
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Account không tồn tại.");

                // 🔐 SECURITY: check ownership
                if (account.UserId != userId)
                    throw new ApplicationException(ErrorCode.Forbidden, "Không có quyền truy cập account.");

                if (!account.IsActive)
                    throw new ApplicationException(ErrorCode.Forbidden, "Account inactive.");

                // 🚫 IGNORE request.AccountNumber / BankCode
                var bank = await _unitOfWork.BankInfos.GetByBankCodeAsync(account.BankCode)
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Bank không tồn tại.");

                return (
                    account.AccountNumber,
                    account.AccountHolder,
                    account.BankCode,
                    bank.BankName,
                    bank.ShortName,
                    bank.NapasBin
                );
            }

            // =========================
            // CASE 2: INPUT TAY
            // =========================

            // 👉 Provider = Napas (VietQR)
            if (mode == QrMode.VietQR)
            {
                if (string.IsNullOrWhiteSpace(request.AccountNumber))
                    throw new ValidationException("AccountNumber required");

                if (string.IsNullOrWhiteSpace(request.BankCode))
                    throw new ValidationException("BankCode required");

                var bank = await _unitOfWork.BankInfos.GetByBankCodeAsync(request.BankCode)
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Bank không tồn tại.");

                return (
                    request.AccountNumber,
                    null, // không có holder
                    request.BankCode,
                    bank.BankName,
                    bank.ShortName,
                    bank.NapasBin
                );
            }

            // 👉 Provider = MoMo Native (phone)
            if (mode == QrMode.MomoNative)
            {
                if (string.IsNullOrWhiteSpace(request.AccountNumber))
                    throw new ValidationException("Phone required");

                return (
                    request.AccountNumber,
                    null,
                    null,
                    null,
                    null,
                    null
                );
            }

            // 👉 fallback
            throw new ApplicationException(ErrorCode.InternalError, "Mode không hỗ trợ");
        }

        private async Task<Provider> GetProviderAsync(Guid providerId)
        {
            var provider = await _unitOfWork.Providers.GetByIdAsync(providerId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Provider không tồn tại.");

            if (!provider.IsActive)
                throw new ApplicationException(ErrorCode.ServiceUnavailable, "Phương thức thanh toán đang bảo trì.");

            return provider;
        }
        #endregion
    }
}
