using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Banks.Responses;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Common.Mapper
{
    public class BankInfoMapper
    {
        public static GetBankInfoRes ToGetBankInfoRes(BankInfo u, IFileStorageService? fileStorageService = null)
        {
            return new GetBankInfoRes
            {
                Id = u.Id,
                BankCode = u.BankCode,
                NapasBin = u.NapasBin,
                SwiftCode = u.SwiftCode,

                BankName = u.BankName,
                ShortName = u.ShortName,
                LogoUrl = BaseMapper.ResolveFileUrl(u.LogoUrl, fileStorageService),
                IsActive = u.IsActive,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,
            };
        }
    }
}
