using Carts.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface ICartProductRepository : IBaseRepository<CartProduct>
    {
        Task<CartProduct?> ViewByComplexIdAsync(int cartId, int productId);
        Task<CartProduct?> GetByComplexIdAsync(int cartId, int productId);
        Task<List<CartProduct?>> GetAllCartProductsRowsAsync(int cartId);
    }
}
