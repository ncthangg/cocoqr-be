using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Roles.Responses;
using CocoQR.Application.DTOs.UserRoles.Requests;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
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
