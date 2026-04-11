using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoQR.Application.Contracts.IRateLimit
{
    public interface IRateLimitService
    {
        Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window);
    }
}
