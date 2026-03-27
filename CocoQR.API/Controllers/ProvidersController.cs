using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Providers.Requests;
using CocoQR.Application.DTOs.Providers.Responses;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvidersController : ControllerBase
    {
        private readonly IProviderService _providerService;
        public ProvidersController(IProviderService providerService)
        {
            _providerService = providerService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            IEnumerable<GetProviderRes> result = await _providerService.GetAllAsync();

            return Ok(new BaseResponseModel<IEnumerable<GetProviderRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, [FromForm] PutProviderReq request)
        {
            await _providerService.PutAsync(id, request);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }
    }
}
