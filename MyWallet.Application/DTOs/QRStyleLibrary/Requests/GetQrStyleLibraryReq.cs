using MyWallet.Domain.Constants.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.DTOs.QRStyleLibrary.Requests
{
    public class GetQrStyleLibraryReq
    {
        public Guid? UserId { get; set; }
        public QRStyleType? Type { get; set; }
        public bool? IsActive { get; set; }
    }
}
