using Microsoft.EntityFrameworkCore;
using Orders.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<OrderItem?> ViewByComplexIdAsync(int orderId,int productId)
        {
            return await _dbContext.OrderItems.AsNoTracking().FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId);
        }
    }
}
