using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.QR.Requests;
using MyWallet.Application.DTOs.QR.Responses;
using MyWallet.Domain.Constants;

namespace MyWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRController : Controller
    {
        private readonly IQrService _qrService;
        public QRController(IQrService qrService)
        {
            _qrService = qrService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostQrReq request)
        {
            var result = await _qrService.CreateAsync(request);

            return Ok(new BaseResponseModel<GetQrRes>(
                      code: SuccessCode.Success,
                      data: result,
                      message: null));
        }
    }
}
