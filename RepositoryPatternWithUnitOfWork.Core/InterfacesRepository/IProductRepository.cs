using Carts.Entities;
using NPOI.SS.Formula.Functions;
using Products.DTOs;
using Products.Entities;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;
using Users.DTOs;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> ViewByIdAsync(int id);
        Task<List<Product>> GetProductsByName(string name);
        Task<List<Product>> GetBySellerIdAsync(int sellerId);
    }
}
