using MyWallet.Application.DTOs.QRStyleLibrary.Requests;
using MyWallet.Application.DTOs.QRStyleLibrary.Responses;
using MyWallet.Domain.Constants.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IQrStyleLibraryService
    {
        Task<IEnumerable<GetQrStyleLibraryRes>> GetAllAsync(QRStyleType? type, bool? isActive);

        Task<Guid> PostUserStyleAsync(PostQRStyleReq request);
        Task PutUserStyleAsync(Guid styleId, PutQRStyleReq request);
        Task DeleteUserStyleAsync(Guid styleId);
    }
}
