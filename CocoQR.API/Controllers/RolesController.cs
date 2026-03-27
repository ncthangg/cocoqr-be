using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Roles.Requests;
using CocoQR.Application.DTOs.Roles.Responses;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            IEnumerable<GetRoleRes> result = await _roleService.GetAllAsync();

            return Ok(new BaseResponseModel<IEnumerable<GetRoleRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, PutRoleReq request)
        {
            await _roleService.PutAsync(id, request);
            return Ok(new BaseResponseModel<string>(
               code: SuccessCode.Success,
               data: null,
               message: SuccessMessages.UpdateSuccess));
        }
    }
}
