using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IQRStyleRepository : IRepository<QRStyle>
    {
        Task<QRStyle?> GetByQrIdAsync(long qrId);
    }
}
