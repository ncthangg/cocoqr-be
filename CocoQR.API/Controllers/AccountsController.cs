using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Accounts.Requests;
using CocoQR.Application.DTOs.Accounts.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
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
        public async Task<IActionResult> Get([FromQuery] GetAccountReq req)
        {
            PagingVM<GetAccountRes> result = await _accountService.GetAllAsync(req.PageNumber, req.PageSize,
                                                                               req.SortField, req.SortDirection,
                                                                               null,
                                                                               req.ProviderId,
                                                                               req.SearchValue,
                                                                               req.IsActive,
                                                                               false,
                                                                               true);
            return Ok(new BaseResponseModel<PagingVM<GetAccountRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpGet("by-admin")]
        [Authorize]
        public async Task<IActionResult> GetByAdmin([FromQuery] GetAccountByAdminReq req)
        {
            PagingVM<GetAccountRes> result = await _accountService.GetAllAsync(req.PageNumber, req.PageSize,
                                                                               req.SortField, req.SortDirection,
                                                                               req.UserId,
                                                                               req.ProviderId,
                                                                               req.SearchValue,
                                                                               req.IsActive,
                                                                               req.IsDeleted,
                                                                               req.Status);
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
        public async Task<IActionResult> Post(PostAccountReq request)
        {
            await _accountService.PostAccountAsync(request);
            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: SuccessMessages.CreateSuccess));
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, PutAccountReq request)
        {
            await _accountService.PutAccountAsync(id, request);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> PutStatus(Guid id)
        {
            await _accountService.PutStatusAsync(id);
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
