using CocoQR.Domain.Constants.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoQR.Application.DTOs.Contacts.Requests
{
    public class PutEmailStatusReq
    {
        public Guid EmailLogId { get; set; }
        public EmailLogStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
