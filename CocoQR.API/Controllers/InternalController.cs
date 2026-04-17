using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Domain.Constants.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InternalController : ControllerBase
    {
        private readonly IContactService _contactService;

        public InternalController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost("mail-log/update-status")]
        public async Task<IActionResult> UpdateMailStatus([FromBody] PutEmailStatusReq req)
        {
            await _contactService.UpdateEmailLogStatusAsync(req.EmailLogId, req.Status, req.ErrorMessage);
            return Ok();
        }
    }
}
