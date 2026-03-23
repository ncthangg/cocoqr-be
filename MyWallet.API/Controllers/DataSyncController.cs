using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.Seed;
using MyWallet.Domain.Constants;
using MyWallet.Infrastructure.Persistence.Seeder;

namespace MyWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class DataSyncController : ControllerBase
    {
        private readonly RoleSeeder _roleSeeder;
        private readonly ProviderSeeder _providerSeeder;
        private readonly BankSeeder _bankSeeder;

        public DataSyncController(RoleSeeder roleSeeder, ProviderSeeder providerSeeder, BankSeeder bankSeeder)
        {
            _roleSeeder = roleSeeder;
            _providerSeeder = providerSeeder;
            _bankSeeder = bankSeeder;
        }

        [HttpPost("roles/preview")]
        public async Task<IActionResult> RolePreview()
        {
            var result = await _roleSeeder.PreviewAsync();
            return Ok(new BaseResponseModel<RoleSyncPreviewRes>(
                   code: SuccessCode.Success,
                   message: null,
                   data: result
                   ));
        }

        [HttpPost("roles")]
        public async Task<IActionResult> SyncRoles()
        {
            await _roleSeeder.SeedAsync();
            return Ok(new BaseResponseModel<string>(
                   code: SuccessCode.Success,
                   message: null,
                   data: null
                   ));
        }

        [HttpPost("providers/preview")]
        public async Task<IActionResult> ProviderPreview()
        {
            var result = await _providerSeeder.PreviewAsync();
            return Ok(new BaseResponseModel<ProviderSyncPreviewRes>(
                   code: SuccessCode.Success,
                   message: null,
                   data: result
                   ));
        }

        [HttpPost("providers")]
        public async Task<IActionResult> SyncProviders()
        {
            await _providerSeeder.SeedAsync();
            return Ok(new BaseResponseModel<string>(
                   code: SuccessCode.Success,
                   message: null,
                   data: null
                   ));
        }

        [HttpPost("banks/preview")]
        public async Task<IActionResult> BankPreview()
        {
            var result = await _bankSeeder.PreviewAsync();
            return Ok(new BaseResponseModel<BankSyncPreviewRes>(
                   code: SuccessCode.Success,
                   message: null,
                   data: result
                   ));
        }

        [HttpPost("banks")]
        public async Task<IActionResult> SyncBanks()
        {
            await _bankSeeder.SeedAsync();
            return Ok(new BaseResponseModel<string>(
                   code: SuccessCode.Success,
                   message: null,
                   data: null
                   ));
        }

    }
}
