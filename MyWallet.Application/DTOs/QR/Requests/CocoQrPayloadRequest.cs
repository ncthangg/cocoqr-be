using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProviderCode = MyWallet.Domain.Constants.Enum.ProviderCode;

namespace MyWallet.Application.DTOs.QR.Requests
{
    public class CocoQrPayloadRequest
    {
        public required string ProviderCode { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string? NapasBin { get; set; }

        public string? ReceiverName { get; set; }

        public decimal? Amount { get; set; }
        public string? Description { get; set; }
    }
}
