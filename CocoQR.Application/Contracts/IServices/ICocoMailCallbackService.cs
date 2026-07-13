using CocoQR.Application.DTOs.CocoMail;

namespace CocoQR.Application.Contracts.IServices
{
    public interface ICocoMailCallbackService
    {
        Task HandleAsync(CocoMailCallbackRequest request, CancellationToken cancellationToken = default);
    }
}
