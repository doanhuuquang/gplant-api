using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentUrlAsync(Order order, string returnUrl, string ipAddress);
        Task<PaymentCallbackResponse> ProcessPaymentCallbackAsync(Dictionary<string, string> queryParams);
        bool ValidatePaymentCallback(Dictionary<string, string> queryParams);
    }
}