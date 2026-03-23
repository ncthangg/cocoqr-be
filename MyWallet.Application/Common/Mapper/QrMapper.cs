using MyWallet.Application.DTOs.QR.Queries;
using MyWallet.Application.DTOs.QR.Responses;
using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Application.Common.Mapper
{
    public class QrMapper
    {
        public static GetQrRes ToGetQrHistoryRes(QrHistoryQueryDto q)
        {
            return new GetQrRes
            {
                Id = q.Id,
                UserId = q.UserId ?? null,
                AccountNumberSnapshot = q.AccountNumberSnapshot ?? null,
                AccountHolderSnapshot = q.AccountHolderSnapshot ?? null,

                BankCodeSnapshot = q.BankCodeSnapshot ?? null,
                BankNameSnapshot = q.BankNameSnapshot ?? null,
                BankShortNameSnapshot = q.BankShortNameSnapshot ?? null,
                NapasBinSnapshot = q.NapasBinSnapshot ?? null,

                QrData = q.QrData ?? null,
                TransactionRef = q.TransactionRef ?? null,

                ProviderId = q.ProviderId,
                ProviderCode = q.ProviderCode,
                ProviderName = q.ProviderName ?? null,
                ProviderLogoUrl = q.ProviderLogoUrl ?? null,

                Amount = q.Amount ?? null,
                Currency = q.Currency ?? null,
                Description = q.Description ?? null,

                ReceiverType = q.ReceiverType ?? QRReceiverType.GUEST,
                IsFixedAmount = q.IsFixedAmount ?? null,
                QrMode = q.QrMode ?? null,
                QrStatus = q.QrStatus ?? null,

                CreatedAt = q.CreatedAt ?? null,
            };
        }
        public static GetQrRes ToGetQrHistoryByAdminRes(QrHistoryQueryDto q)
        {
            return new GetQrRes
            {
                Id = q.Id,
                UserId = q.UserId ?? null,
                Email = q.Email ?? null,
                AccountNumberSnapshot = q.AccountNumberSnapshot ?? null,
                AccountHolderSnapshot = q.AccountHolderSnapshot ?? null,

                BankCodeSnapshot = q.BankCodeSnapshot ?? null,
                BankNameSnapshot = q.BankNameSnapshot ?? null,
                BankShortNameSnapshot = q.BankShortNameSnapshot ?? null,
                NapasBinSnapshot = q.NapasBinSnapshot ?? null,

                QrData = q.QrData ?? null,
                TransactionRef = q.TransactionRef ?? null,

                ProviderId = q.ProviderId,
                ProviderCode = q.ProviderCode,
                ProviderName = q.ProviderName ?? null,
                ProviderLogoUrl = q.ProviderLogoUrl ?? null,

                Amount = q.Amount ?? null,
                Currency = q.Currency ?? null,
                Description = q.Description ?? null,

                ReceiverType = q.ReceiverType ?? QRReceiverType.GUEST,
                IsFixedAmount = q.IsFixedAmount ?? null,
                QrMode = q.QrMode ?? null,
                QrStatus = q.QrStatus ?? null,

                CreatedAt = q.CreatedAt ?? null,
                ExpiredAt = q.ExpiredAt ?? null,
                PaidAt = q.PaidAt ?? null,
                DeletedAt = q.DeletedAt ?? null,
            };
        }
    }
}
