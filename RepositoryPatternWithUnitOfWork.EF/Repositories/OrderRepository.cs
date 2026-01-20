using Microsoft.EntityFrameworkCore;
using Orders.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class OrderRepository : BaseRepository<Order> , IOrderRepository
    {
        public OrderRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Order?> ViewByIdAsync(int id)
        {
            return await _dbContext.Orders.Include(o => o.OrderItems).AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await _dbContext.Orders.Include(o => o.OrderItems).AsSplitQuery().FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
