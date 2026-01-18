using Orders.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.Core.InterfacesRepository
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        public Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        public Task<List<Payment>> GetPaymentsByUserIdAsync(int userId);
    }
}
