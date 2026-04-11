using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.EmailLogs.Requests;
using CocoQR.Application.DTOs.EmailLogs.Responses;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class EmailLogsController : ControllerBase
    {
        private readonly IEmailLogService _emailLogService;

        public EmailLogsController(IEmailLogService emailLogService)
        {
            _emailLogService = emailLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetEmailLogReq request)
        {
            var result = await _emailLogService.GetAsync(request);

            return Ok(new BaseResponseModel<PagingVM<GetEmailLogRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _emailLogService.GetByIdAsync(id);

            return Ok(new BaseResponseModel<GetEmailLogByIdRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
    }
}