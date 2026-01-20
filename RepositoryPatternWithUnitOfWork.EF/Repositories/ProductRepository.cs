using Microsoft.EntityFrameworkCore;
using Products.DTOs;
using Products.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using Result_Pattern;
using Users.DTOs;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {


        public ProductRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {

        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbContext.Products.Include(p => p.ProductImages).AsSplitQuery().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> ViewByIdAsync(int id)
        {
            return await _dbContext.Products.Include(p => p.ProductImages).AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<List<Product>> GetProductsByName(string name)
        {
            return await _dbContext.Products.AsNoTracking().Where(p => p.ProductName.Contains(name)).ToListAsync();
        }
        public async Task<List<Product>> GetBySellerIdAsync(int sellerId)
        {
            return await _dbContext.Products.AsNoTracking().Where(p => p.SellerId == sellerId).ToListAsync();
        }
    }
}




