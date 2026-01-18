using Carts.Entities;
using Orders.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface IOrderItemRepository : IBaseRepository<OrderItem>
    {
        Task<OrderItem?> ViewByComplexIdAsync(int orderId, int productId);

    }
}
