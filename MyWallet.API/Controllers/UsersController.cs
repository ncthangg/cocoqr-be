using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;
using MyWallet.Domain.Constants;

namespace MyWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
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
    }
}
