using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Banks.Requests;
using CocoQR.Application.DTOs.Banks.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankInfosController : ControllerBase
    {
        private readonly IBankInfoService _bankInfoService;
        public BankInfosController(IBankInfoService bankInfoService)
        {
            _bankInfoService = bankInfoService;
        }
        [HttpGet]
        public async Task<IActionResult> Get(int pageNumber = 1, int pageSize = 10, string? sortField = null, string? sortDirection = null, bool? isActive = null, string? searchValue = null)
        {
            PagingVM<GetBankInfoRes> result = await _bankInfoService.GetsAsync(pageNumber, pageSize, sortField, sortDirection, isActive, searchValue);

            return Ok(new BaseResponseModel<PagingVM<GetBankInfoRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, [FromForm] PutBankInfoReq request)
        {
            await _bankInfoService.PutAsync(id, request);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }

    }
}