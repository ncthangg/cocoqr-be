using MyWallet.Application.DTOs.Providers.NewFolder;
using MyWallet.Application.DTOs.Providers.Responses;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Common.Mapper
{
    public class ProviderMapper
    {
        public static GetProviderRes ToGetProviderRes(Provider u)
        {
            return new GetProviderRes
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                IsActive = u.IsActive,
                LogoUrl = u.LogoUrl,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,

                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy,
                DeletedBy = u.DeletedBy,
            };
        }
        public static GetProviderRes ToGetProviderRes(ProviderQueryDto u)
        {
            return new GetProviderRes
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                IsActive = u.IsActive,
                LogoUrl = u.LogoUrl,

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,

                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy,

                CreatedByName = u.CreatedByName,
                UpdatedByName = u.UpdatedByName,
            };
        }
    }
}
