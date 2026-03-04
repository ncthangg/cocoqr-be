using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;
using MyWallet.Application.Services;
using MyWallet.Domain.Constants;

namespace MyWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        public UserRoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get(int pageNumber = 1, int pageSize = 10, Guid? roleId = null)
        {
            PagingVM<GetUserRoleRes> result = await _userRoleService.GetAllUserRoles(pageNumber, pageSize, roleId);
            return Ok(new BaseResponseModel<PagingVM<GetUserRoleRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpGet("users/{userId}/roles")]
        [Authorize]
        public async Task<IActionResult> GetRolesByUserId(Guid userId)
        {
            var result = await _userRoleService.GetRolesByUserIdAsync(userId);

            return Ok(new BaseResponseModel<IEnumerable<GetRoleRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddUserToRole([FromBody] AddUserRoleReq req)
        {
            var result = await _userRoleService.AddUserToRoleAsync(req);

            return Ok(new BaseResponseModel<bool>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveUserFromRole([FromQuery] Guid userId, [FromQuery] Guid roleId)
        {
            var result = await _userRoleService.RemoveUserFromRoleAsync(userId, roleId);

            return Ok(new BaseResponseModel<bool>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
    }
}
