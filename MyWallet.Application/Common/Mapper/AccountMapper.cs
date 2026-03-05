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

                CreatedByName = BaseMapper.GetUserName(u.CreatedBy, userDict),
                UpdatedByName = BaseMapper.GetUserName(u.UpdatedBy, userDict),
                DeletedByName = BaseMapper.GetUserName(u.DeletedBy, userDict),
            };
        }

    }
}
