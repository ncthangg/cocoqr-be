using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.QR.Requests;
using CocoQR.Application.DTOs.QR.Responses;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRController : ControllerBase
    {
        private readonly IQrService _qrService;
        public QRController(IQrService qrService)
        {
            _qrService = qrService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] GetQrReq req)
        {
            PagingVM<GetQrRes> result = await _qrService.GetAllAsync(req.PageNumber, req.PageSize,
                                                                     req.SortField, req.SortDirection,
                                                                     null,
                                                                     req.ProviderId,
                                                                     req.SearchValue,
                                                                     false,
                                                                     true);
            return Ok(new BaseResponseModel<PagingVM<GetQrRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpGet("by-admin")]
        [Authorize]
        public async Task<IActionResult> GetByAdmin([FromQuery] GetQrByAdminReq req)
        {
            PagingVM<GetQrRes> result = await _qrService.GetAllAsync(req.PageNumber, req.PageSize,
                                                                          req.SortField, req.SortDirection,
                                                                          req.UserId,
                                                                          req.ProviderId,
                                                                          req.SearchValue,
                                                                          req.IsDeleted,
                                                                          req.Status);
            return Ok(new BaseResponseModel<PagingVM<GetQrRes>>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {
            GetQrRes result = await _qrService.GetByIdAsync(id);
            return Ok(new BaseResponseModel<GetQrRes>(
                code: SuccessCode.Success,
                data: result,
                message: null));
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostQrReq request)
        {
            var result = await _qrService.GenerateAsync(request);

            return Ok(new BaseResponseModel<PostQrRes>(
                      code: SuccessCode.Success,
                      data: result,
                      message: null));
        }
    }
}
