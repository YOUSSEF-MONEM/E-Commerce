using Microsoft.EntityFrameworkCore;
using Orders.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using RepositoryPatternWithUnitOfWork.Core.InterfacesRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class PaymentRepository :BaseRepository<Payment>, IPaymentRepository 
    {
        public PaymentRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {

        }
        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<List<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _dbContext.Payments.Where(p => p.UserId == userId).ToListAsync();
        }
    }
}
