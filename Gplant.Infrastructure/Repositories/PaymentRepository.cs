using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Gplant.Infrastructure.Repositories
{
    public class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
    {
        public async Task<List<Payment>> GetByOrderIdAsync(Guid orderId)
            => await context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync();

        public async Task<Payment?> GetLatestByOrderIdAsync(Guid orderId)
            => await context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .FirstOrDefaultAsync();

        public async Task<Payment?> GetByGatewayTransactionIdAsync(string transactionId)
            => await context.Payments
                .FirstOrDefaultAsync(p => p.GatewayTransactionId == transactionId);

        public async Task CreateAsync(Payment payment)
        {
            await context.Payments.AddAsync(payment);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            context.Payments.Update(payment);
            await context.SaveChangesAsync();
        }
    }
}