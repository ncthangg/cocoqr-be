using MyWallet.Application.DTOs.Response;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Common.Mapper
{
    public class BankInfoMapper
    {
        public static GetBankInfoRes ToGetBankInfoRes(BankInfo u, Dictionary<Guid, string>? userDict)
        {
            return new GetBankInfoRes
            {
                Id = u.Id,
                BankCode = u.BankCode,
                NapasCode = u.NapasCode,
                SwiftCode = u.SwiftCode,
                BankName = u.BankName,

                ShortName = u.ShortName,
                LogoUrl = u.LogoUrl,
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
