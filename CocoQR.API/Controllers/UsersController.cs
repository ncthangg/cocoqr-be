using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Users.Requests;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] GetUserReq req)
        {
            PagingVM<GetUserBaseRes> result = await _userService.GetUsersAsync(
                req.PageNumber,
                req.PageSize,
                req.SortField,
                req.SortDirection,
                req.Status,
                req.SearchValue,
                req.RoleId);

            return Ok(new BaseResponseModel<PagingVM<GetUserBaseRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            GetUserBySystemRes result = await _userService.GetByIdAsync(id);
            return Ok(new BaseResponseModel<GetUserBySystemRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<IActionResult> PatchStatus(Guid id)
        {
            await _userService.PatchStatusAsync(id);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }
    }
}
