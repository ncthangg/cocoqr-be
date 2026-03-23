using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.Roles.Responses;
using MyWallet.Application.DTOs.UserRoles.Requests;
using MyWallet.Domain.Constants;

namespace MyWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        public UserRolesController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }
        [HttpGet("{userId}/roles")]
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
        public async Task<IActionResult> PostPutUserRoles([FromBody] PostPutUserRoleReq req)
        {
            var result = await _userRoleService.PostPutUserRolesAsync(req);

            return Ok(new BaseResponseModel<bool>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
    }
}
