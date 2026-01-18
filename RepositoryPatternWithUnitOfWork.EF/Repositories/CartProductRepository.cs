using Carts.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;
using Users.DTOs;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class CartProductRepository : BaseRepository<CartProduct>, ICartProductRepository
    {
        public CartProductRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<CartProduct?> ViewByComplexIdAsync(int cartId , int productId)
        {
            return await _dbContext.CartProducts.AsNoTracking()
                .FirstOrDefaultAsync(e => e.CartId == cartId && e.ProductId == productId );
        }

        public async Task<CartProduct?> GetByComplexIdAsync(int cartId , int productId)
        {
            var cart =  await _dbContext.CartProducts
                .FirstOrDefaultAsync(e => e.CartId == cartId && e.ProductId == productId );
            return cart;
        }

        public async Task<List<CartProduct?>> GetAllCartProductsRowsAsync(int cartId)
        {
            var cartProductRows = await _dbContext.CartProducts.Where(e => e.CartId == cartId)
                .ToListAsync();

            return cartProductRows!;
        }
    }
}