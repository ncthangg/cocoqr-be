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
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get(Guid userId, int pageNumber = 1, int pageSize = 10, bool? isActive = true)
        {
            PagingVM<GetAccountRes> result = await _accountService.GetUserAccountsAsync(userId, pageNumber, pageSize, isActive);
            return Ok(new BaseResponseModel<PagingVM<GetAccountRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            GetAccountRes result = await _accountService.GetByIdAsync(id);
            return Ok(new BaseResponseModel<GetAccountRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromForm] PostAccountReq request)
        {
            await _accountService.PostAccountAsync(request);
            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.CreateSuccess));
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, [FromForm] PutAccountReq request)
        {
            await _accountService.PutAccountAsync(id, request);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _accountService.DeleteAccountAsync(id);
            return Ok(new BaseResponseModel<string>(
              code: SuccessCode.Success,
              data: null,
              message: SuccessMessages.DeleteForeverSuccess));
        }
    }
}
