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
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplateService _emailTemplateService;

        public EmailTemplatesController(IEmailTemplateService emailTemplateService)
        {
            _emailTemplateService = emailTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _emailTemplateService.GetAllAsync();

            return Ok(new BaseResponseModel<IEnumerable<GetEmailTemplateRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _emailTemplateService.GetByIdAsync(id);

            return Ok(new BaseResponseModel<GetEmailTemplateRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostEmailTemplateReq request)
        {
            var result = await _emailTemplateService.PostAsync(request);

            return Ok(new BaseResponseModel<Guid>(
                code: SuccessCode.Success,
                data: result,
                message: SuccessMessages.CreateSuccess));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] PutEmailTemplateReq request)
        {
            await _emailTemplateService.PutAsync(id, request);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.UpdateSuccess));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _emailTemplateService.DeleteAsync(id);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.DeleteSuccess));
        }
    }
}
