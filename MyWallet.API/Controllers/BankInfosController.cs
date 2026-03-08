using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;
using MyWallet.Domain.Constants;

namespace MyWallet.API.Controllers
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
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            GetBankInfoRes result = await _bankInfoService.GetByIdAsync(id);
            return Ok(new BaseResponseModel<GetBankInfoRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromForm] PostBankInfoReq request)
        {
            await _bankInfoService.PostAsync(request);
            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.CreateSuccess));
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
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bankInfoService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string>(
              code: SuccessCode.Success,
              data: null,
              message: SuccessMessages.DeleteForeverSuccess));
        }
    }
}