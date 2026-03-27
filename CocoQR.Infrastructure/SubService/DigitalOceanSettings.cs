using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoQR.Infrastructure.SubService
{
    public class DigitalOceanSettings
    {
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string Bucket { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string Endpoint { get; set; } = default!;
    }
}
