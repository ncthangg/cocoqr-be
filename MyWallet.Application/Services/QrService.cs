using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.Contracts.IUnitOfWork;
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
        private readonly IIdGenerator _idGenerator;

        private readonly IFileStorageService _fileStorageService;

        private readonly IQrPayloadEngine _qrEngine;
        private readonly IQrImageRenderer _qrImageRenderer;


        public QrService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, IFileStorageService fileStorageService,
                    IQrPayloadEngine qrEngine,
                    IQrImageRenderer qrImageRenderer)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext;
            _idGenerator = idGenerator;

            _fileStorageService = fileStorageService;

            _qrEngine = qrEngine;
            _qrImageRenderer = qrImageRenderer;
        }

        public async Task<GetQrRes> CreateAsync(PostQrReq request)
        {
            // 1. Resolve account info
            var (accountNumber, bankCode, accountHolder) = await ResolveAccountAsync(request);

            // 2. Resolve provider để xác định Mode
            var provider = await GetProviderAsync(request.ProviderId);
            // var mode = ResolveMode(provider.Code, request.QrMode);

            // 3. Generate EMVCo payload qua engine
            var engineRequest = new QrGenerateRequest
            {
                ProviderCode = provider.Code,
                BankCode = bankCode,
                AccountNumber = accountNumber,
                Amount = request.Amount,
                Description = request.Description,
                IsStatic = true, // fixed amount → dynamic
                Mode = QrMode.VietQR,
            };

            var payloadResult = _qrEngine.Generate(engineRequest);

            // 4. Render QR image
            var imageResult = _qrImageRenderer.Render(new QrImageRequest
            {
                Payload = payloadResult.Payload,
                SizePixels = 400,
            });

            // 5. Persist — chỉ lưu payload string, KHÔNG lưu ảnh vào DB
            var entity = new QRHistory
            {
                UserId = _userContext.UserId,
                AccountNumberSnapshot = accountNumber,
                AccountHolderSnapshot = accountHolder,

                BankCodeSnapshot = bankCode ?? null,
                NapasBinSnapshot = payloadResult.NapasBin,

                Amount = request.Amount,
                Description = request.Description,

                QRData = payloadResult.Payload,
                TransactionRef = GenerateTransactionRef(),

                ProviderId = provider.Id,
                IsFixedAmount = request.IsFixedAmount,
                QrMode = QrMode.VietQR,
                CreatedAt = DateTime.UtcNow,
            };

            var id = await _unitOfWork.QRHistories.Post(entity);

            // 6. Return — ảnh generate on-the-fly, không lưu DB
            return new GetQrRes
            {
                Id = id,
                QrData = payloadResult.Payload,
                QrImageUrl = imageResult.DataUri,  // "data:image/png;base64,..."
                TransactionRef = entity.TransactionRef,
                IsValid = payloadResult.IsValid,
            };
        }

        public async Task<GetQrRes> RegenerateImageAsync(Guid qrHistoryId)
        {
            // Lấy payload từ DB → render lại ảnh — không cần lưu ảnh
            var history = await _unitOfWork.QRHistories.GetByIdAsync(qrHistoryId)
                ?? throw new ApplicationException(ErrorCode.NotFound, "QR history không tồn tại.");

            var imageResult = _qrImageRenderer.Render(new QrImageRequest
            {
                Payload = history.QRData,
            });

            return new GetQrRes
            {
                Id = history.Id,
                QrData = history.QRData,
                QrImageUrl = imageResult.DataUri,
                TransactionRef = history.TransactionRef,
                IsValid = _qrEngine.Verify(history.QRData),
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

        private async Task<(string accountNumber, string bankCode, string? holder)>
            ResolveAccountAsync(PostQrReq request)
        {
            if (request.AccountId.HasValue)
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId.Value)
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Account không tồn tại.");

                if (!account.IsActive)
                    throw new ApplicationException(ErrorCode.Unauthorized, "Tài khoản không được phép tạo mã.");

                return (account.AccountNumber, account.BankCode, account.AccountHolder);
            }

            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ValidationException("AccountNumber bắt buộc.");

            return (request.AccountNumber, request.BankCode ?? "", null);
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
