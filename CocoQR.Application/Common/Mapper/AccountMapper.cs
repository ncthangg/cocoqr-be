using CocoQR.Application.DTOs.Accounts.Queries;
using CocoQR.Application.DTOs.Accounts.Responses;

namespace CocoQR.Application.Common.Mapper
{
    public class AccountMapper
    {
        public static GetAccountRes ToGetAccountRes(AccountQueryDto u)
        {
            return new GetAccountRes
            {
                Id = u.Id,
                UserId = u.UserId,
                AccountNumber = u.AccountNumber,
                AccountHolder = u.AccountHolder ?? null,

                BankCode = u.BankCode ?? null,
                NapasBin = u.NapasBin ?? null,
                BankName = u.BankName ?? null,
                BankShortName = u.BankShortName ?? null,
                BankLogoUrl = u.BankLogoUrl ?? null,

                BankIsActive = u.BankIsActive ?? null,

                ProviderId = u.ProviderId,
                ProviderCode = u.ProviderCode,
                ProviderName = u.ProviderName ?? null,
                ProviderLogoUrl = u.ProviderLogoUrl ?? null,

                ProviderIsActive = u.ProviderIsActive,

                Balance = u.Balance ?? null,

                IsPinned = u.IsPinned,
                IsActive = u.IsActive,

                CreatedAt = u.CreatedAt,
            };
        }
        public static GetAccountRes ToGetAccountByAdminRes(AccountQueryDto u)
        {
            return new GetAccountRes
            {
                Id = u.Id,
                UserId = u.UserId,
                AccountNumber = u.AccountNumber,
                AccountHolder = u.AccountHolder ?? null,

                BankCode = u.BankCode ?? null,
                NapasBin = u.NapasBin ?? null,
                BankName = u.BankName ?? null,
                BankShortName = u.BankShortName ?? null,
                BankLogoUrl = u.BankLogoUrl ?? null,

                BankIsActive = u.BankIsActive ?? null,
                BankStatus = u.BankStatus ?? null,

                ProviderId = u.ProviderId,
                ProviderCode = u.ProviderCode,
                ProviderName = u.ProviderName ?? null,
                ProviderLogoUrl = u.ProviderLogoUrl ?? null,

                ProviderIsActive = u.ProviderIsActive,
                ProviderStatus = u.ProviderStatus,

                IsPinned = u.IsPinned,
                IsActive = u.IsActive,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,

                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy,
                DeletedBy = u.DeletedBy,

                CreatedByName = u.CreatedByName,
                UpdatedByName = u.UpdatedByName,
                DeletedByName = u.DeletedByName,
            };
        }
    }
}
