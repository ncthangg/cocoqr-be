using Microsoft.AspNetCore.Mvc;
using CocoQR.QR_Decoder.QR.Dto;
using CocoQR.QR_Decoder.QR.Interface;

namespace CocoQR.QR_Decoder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DecodeController : ControllerBase
    {
        private readonly IQrDecoderService _qrDecoder;

        public DecodeController(IQrDecoderService qrDecoder)
        {
            _qrDecoder = qrDecoder;
        }

        [HttpPost]
        public async Task<IActionResult> Decode(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var payload = await _qrDecoder.DecodeAsync(stream);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return BadRequest("Unable to decode QR payload from the provided image.");
            }

            var vietQrInfo = _qrDecoder.ParsePayload(payload);

            return Ok(new QrDecodeResult()
            {
                VietQrInfo = vietQrInfo
            });
        }
    }
}
