using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Application.Interfaces
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(Order order, string clientIp);
        bool ValidateSignature(IQueryCollection query);
        string GetResponseCode(IQueryCollection query);
        string GetOrderCode(IQueryCollection query);
        string GetTransactionId(IQueryCollection query);
    }
}
