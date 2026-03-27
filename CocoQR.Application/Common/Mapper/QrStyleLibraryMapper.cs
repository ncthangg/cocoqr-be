using CocoQR.Application.DTOs.QRStyleLibrary.Responses;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Common.Mapper
{
    public class QrStyleLibraryMapper
    {
        public static GetQrStyleLibraryRes ToGetQrStyleLibRes(QRStyleLibrary u)
        {
            return new GetQrStyleLibraryRes
            {
                Id = u.Id,
                Name = u.Name,
                StyleJson = u.StyleJson,
                IsDefault = u.IsDefault,

                Type = u.Type,
                IsActive = u.IsActive,

                CreatedAt = u.CreatedAt,
            };
        }
    }
}
