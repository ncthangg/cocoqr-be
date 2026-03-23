using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Application.DTOs.Providers.Requests
{
    public class PostProviderJsonReq
    {
        public ProviderCode Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}
