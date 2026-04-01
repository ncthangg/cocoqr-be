using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.QRStyleLibrary.Requests;
using CocoQR.Application.DTOs.QRStyleLibrary.Responses;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrStyleLibrariesController : ControllerBase
    {
        private readonly IQrStyleLibraryService _qrStyleLibService;
        public QrStyleLibrariesController(IQrStyleLibraryService qrStyleLibService)
        {
            _qrStyleLibService = qrStyleLibService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] GetQrStyleLibraryReq req)
        {
            IEnumerable<GetQrStyleLibraryRes> result = await _qrStyleLibService.GetAllAsync(req.Type, req.IsActive);
            return Ok(new BaseResponseModel<IEnumerable<GetQrStyleLibraryRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] PostQRStyleReq request)
        {
            var id = await _qrStyleLibService.PostUserStyleAsync(request);

            return Ok(new BaseResponseModel<Guid>(
                code: SuccessCode.Success,
                data: id,
                message: SuccessMessages.CreateSuccess));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, [FromBody] PutQRStyleReq request)
        {
            await _qrStyleLibService.PutUserStyleAsync(id, request);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.UpdateSuccess));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _qrStyleLibService.DeleteUserStyleAsync(id);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.DeleteForeverSuccess));
        }

    }
}
