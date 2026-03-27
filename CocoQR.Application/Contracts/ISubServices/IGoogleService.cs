using CocoQR.Application.DTOs.Auths.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;

namespace CocoQR.Application.Contracts.ISubServices
{
    public interface IGoogleService
    {
        string BuildSuccessHtml(BaseResponseModel<SignInGoogleRes> response, string origin);
    }
}
