using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Application.Interfaces
{
    public interface IQRPaymentService
    {
        Task<string> GenerateVietQRCode(decimal amount, string description);
    }
}
