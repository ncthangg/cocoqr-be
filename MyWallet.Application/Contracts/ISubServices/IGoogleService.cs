using MyWallet.Application.DTOs.Auths.Responses;
using MyWallet.Application.DTOs.Base.BaseRes;

namespace MyWallet.Application.Contracts.ISubServices
{
    public interface IGoogleService
    {
        string BuildSuccessHtml(BaseResponseModel<SignInGoogleRes> response, string origin);
    }
}
