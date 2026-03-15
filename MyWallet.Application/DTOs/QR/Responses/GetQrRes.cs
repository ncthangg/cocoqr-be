using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.QR.Responses
{
    public class GetQrRes
    {
        public long Id { get; set; }
        public string? QrData { get; set; }
        public string? QrImageUrl { get; set; }
        public string? TransactionRef { get; set; }
    }
}
