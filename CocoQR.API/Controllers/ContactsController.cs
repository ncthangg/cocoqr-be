using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Contact([FromBody] ContactRequest request)
        {
            await _contactService.ContactToSystemAsync(request);

            return Ok(new BaseResponseModel<string>(
                code: SuccessCode.Success,
                data: null,
                message: "Gửi liên hệ thành công"));
        }
    }
}