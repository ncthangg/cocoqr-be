using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Domain.Entities
{
    public class QRStyle
    {
        public QRStyle() { }
        public Guid Id { get; set; }
        public long QrId { get; set; }   // FK -> QRHistory
        public string StyleJson { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public virtual QRHistory QR { get; set; } = null!;
    }
}
