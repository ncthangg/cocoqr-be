using MyWallet.Domain.Constants.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.QRStyleLibrary.Responses
{
    public class GetQrStyleLibraryRes
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string StyleJson { get; set; } = null!;

        public bool IsDefault { get; set; }

        public QRStyleType Type { get; set; } // System / User
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
