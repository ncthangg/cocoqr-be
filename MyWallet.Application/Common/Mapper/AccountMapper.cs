using MyWallet.Application.DTOs.Response;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Common.Mapper
{
    public class AccountMapper
    {
        public static GetAccountRes ToGetAccountRes(Account u, Dictionary<Guid, string>? userDict)
        {
            return new GetAccountRes
            {
                Id = u.Id,
                AccountNumber = u.AccountNumber,
                AccountHolder = u.AccountHolder,
                BankCode = u.BankCode,
                BankName = u.BankName,

                AccountType = u.AccountType,
                Balance = u.Balance,
                IsActive = u.IsActive,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,

                CreatedByName = GetUserName(u.CreatedBy, userDict),
                UpdatedByName = GetUserName(u.UpdatedBy, userDict),
                DeletedByName = GetUserName(u.DeletedBy, userDict),
            };
        }
        private static string? GetUserName(
                Guid? userId,
                IReadOnlyDictionary<Guid, string> dict)
        {
            if (userId == null || userId == Guid.Empty) { return null; }
            return dict.TryGetValue(userId.Value, out var name) ? name : null;
        }
    }
}
