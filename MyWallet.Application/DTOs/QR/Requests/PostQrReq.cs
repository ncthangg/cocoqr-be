using MyWallet.Domain.Constants.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.QR.Requests
{
    public class PostQrReq
    {
        public required Guid ProviderId { get; set; }

        public Guid? AccountId { get; set; }
        public string? AccountNumber { get; set; }

        public string? BankCode { get; set; }

        public decimal? Amount { get; set; }
        public string? Description { get; set; }

        public bool IsFixedAmount { get; set; }
    }
}
