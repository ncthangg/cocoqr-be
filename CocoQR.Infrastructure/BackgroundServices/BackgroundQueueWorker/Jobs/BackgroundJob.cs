using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs
{
    public abstract class BackgroundJob
    {
        public Guid JobId { get; set; } = Guid.NewGuid();
        public string JobType { get; set; } = string.Empty;
        public int Attempt { get; set; } = 0;
        public int MaxRetry { get; set; } = 5;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
