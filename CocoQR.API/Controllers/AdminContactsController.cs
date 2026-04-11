using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Application.DTOs.Contacts.Responses;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminContactsController : ControllerBase
    {
        private readonly IContactService _contactService;

        public AdminContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetContactByAdminReq req)
        {
            var result = await _contactService.GetAllAsync(
                req.PageNumber,
                req.PageSize,
                req.SortField,
                req.SortDirection,
                null,
                null,
                null,
                null,
                req.ContactStatus,
                req.FromDate,
                req.ToDate);

            return Ok(new BaseResponseModel<PagingVM<GetContactMessageRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _contactService.GetByIdAsync(id);

            return Ok(new BaseResponseModel<GetContactMessageRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }

        [HttpPost]
        public async Task<IActionResult> Contact([FromBody] AdminContactRequest request)
        {
            await _contactService.ContactFromSystemAsync(request);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: "Gửi liên hệ thành công"));
        }

        [HttpPatch("{id:guid}/ignore")]
        public async Task<IActionResult> Ignore([FromRoute] Guid id)
        {
            await _contactService.IgnoreContactMessageAsync(id);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: "Đã bỏ qua liên hệ"));
        }
    }
}
