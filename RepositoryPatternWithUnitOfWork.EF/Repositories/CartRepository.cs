using Carts.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using Products.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        public virtual async Task<Cart?> ViewByIdAsync(int id)
        {
            return await _dbContext.Carts.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        }

        public virtual async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _dbContext.Carts.Include(c => c.CartProducts).FirstOrDefaultAsync(e => e.UserId == userId);
        }
        //public  async Task<Cart?> GetByUserIdAsync(int userId)
        //{
        //    return await _dbContext.Carts.Include(c => c.CartProducts).FirstOrDefaultAsync(e => e.UserId == userId);
        //}



    }
}


