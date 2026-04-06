using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoQR.Application.Contracts.IQueue
{
    public interface IQueueService
    {
        Task EnqueueAsync<T>(string queue, T data);
        Task<T?> DequeueAsync<T>(string queue);
    }
}
