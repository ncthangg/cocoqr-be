using MyWallet.Application.DTOs.Banks.Responses;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Common.Mapper
{
    public class BankInfoMapper
    {
        public static GetBankInfoRes ToGetBankInfoRes(BankInfo u)
        {
            return new GetBankInfoRes
            {
                Id = u.Id,
                BankCode = u.BankCode,
                NapasBin = u.NapasBin,
                SwiftCode = u.SwiftCode,

                BankName = u.BankName,
                ShortName = u.ShortName,
                LogoUrl = u.LogoUrl,
                IsActive = u.IsActive,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,
            };
        }
    }
}
