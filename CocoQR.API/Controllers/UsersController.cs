using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Constants;

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
        public async Task<IActionResult> Get(int pageNumber = 1, int pageSize = 10, string? sortField = null, string? sortDirection = null, bool? status = null, string? searchValue = null, Guid? roleId = null)
        {
            PagingVM<GetUserBaseRes> result = await _userService.GetUsersAsync(pageNumber, pageSize, sortField, sortDirection, status, searchValue, roleId);

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
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> PutStatus(Guid id)
        {
            await _userService.PutStatusAsync(id);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }
    }
}
