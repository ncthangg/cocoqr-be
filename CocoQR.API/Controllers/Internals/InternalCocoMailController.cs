using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.CocoMail;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers.Internals
{
    [Route("api/internal/cocomail")]
    [ApiController]
    [AllowAnonymous]
    public class InternalCocoMailController : ControllerBase
    {
        private readonly ICocoMailCallbackService _callbackService;

        public InternalCocoMailController(ICocoMailCallbackService callbackService)
        {
            _callbackService = callbackService;
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback(
            [FromBody] CocoMailCallbackRequest request,
            CancellationToken cancellationToken)
        {
            await _callbackService.HandleAsync(request, cancellationToken);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: null));
        }
    }
}
