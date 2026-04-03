using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Settings;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class SmtpSettingsController : ControllerBase
    {
        private readonly ISmtpSettingService _smtpSettingService;

        public SmtpSettingsController(ISmtpSettingService smtpSettingService)
        {
            _smtpSettingService = smtpSettingService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetSmtpSettingReq request)
        {
            var result = await _smtpSettingService.GetAsync(request);

            return Ok(new BaseResponseModel<IEnumerable<GetSmtpSettingRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PutSmtpSettingReq request)
        {
            var result = await _smtpSettingService.PutAsync(request);

            return Ok(new BaseResponseModel<GetSmtpSettingRes>(
                code: SuccessCode.Success,
                data: result,
                message: SuccessMessages.UpdateSuccess));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _smtpSettingService.DeleteAsync(id);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.DeleteSuccess));
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] TestSmtpSettingReq request)
        {
            await _smtpSettingService.TestAsync(request);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: "Gửi mail test thành công"));
        }
    }
}
