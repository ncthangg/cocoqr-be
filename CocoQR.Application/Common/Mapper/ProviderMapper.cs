using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Providers.Queries;
using CocoQR.Application.DTOs.Providers.Responses;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Common.Mapper
{
    public class ProviderMapper
    {
        public static GetProviderRes ToGetProviderRes(Provider u, IFileStorageService? fileStorageService = null)
        {
            return new GetProviderRes
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                IsActive = u.IsActive,
                LogoUrl = BaseMapper.ResolveFileUrl(u.LogoUrl, fileStorageService),

                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,

                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy,
                DeletedBy = u.DeletedBy,
            };
        }
        public static GetProviderRes ToGetProviderRes(ProviderQueryDto u, IFileStorageService? fileStorageService = null)
        {
            return new GetProviderRes
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                IsActive = u.IsActive,
                LogoUrl = BaseMapper.ResolveFileUrl(u.LogoUrl, fileStorageService),

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
