using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetByOrderIdAsync(Guid orderId);
        Task<Payment?> GetLatestByOrderIdAsync(Guid orderId);
        Task<Payment?> GetByGatewayTransactionIdAsync(string transactionId);
        Task CreateAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
}