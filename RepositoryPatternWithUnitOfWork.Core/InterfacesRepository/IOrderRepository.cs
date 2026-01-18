using Carts.Entities;
using Orders.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> ViewByIdAsync(int id);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
    }
}
