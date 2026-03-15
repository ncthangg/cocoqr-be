using MyWallet.Application.DTOs.Accounts.Queries;
using MyWallet.Application.DTOs.Accounts.Responses;

namespace MyWallet.Application.Common.Mapper
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
                AccountHolder = u.AccountHolder ?? "",

                BankCode = u.BankCode ?? "",
                BankName = u.BankName ?? "",
                BankShortName = u.BankShortName ?? "",
                BankLogoUrl = u.BankLogoUrl ?? "",

                BankIsActive = u.BankIsActive,
                BankStatus = u.BankStatus,

                ProviderId = u.ProviderId,
                ProviderCode = u.ProviderCode,
                ProviderName = u.ProviderName,
                ProviderLogoUrl = u.ProviderLogoUrl,

                ProviderIsActive = u.ProviderIsActive,
                ProviderStatus = u.ProviderStatus,

                Balance = u.Balance,

                IsPinned = u.IsPinned,
                IsActive = u.IsActive,

                Status = u.Status,
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
                AccountHolder = u.AccountHolder ?? "",

                BankCode = u.BankCode ?? "",
                BankName = u.BankName ?? "",
                BankShortName = u.BankShortName ?? "",
                BankLogoUrl = u.BankLogoUrl ?? "",

                BankIsActive = u.BankIsActive,
                BankStatus = u.BankStatus,

                ProviderId = u.ProviderId,
                ProviderCode = u.ProviderCode,
                ProviderName = u.ProviderName,
                ProviderLogoUrl = u.ProviderLogoUrl,

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
