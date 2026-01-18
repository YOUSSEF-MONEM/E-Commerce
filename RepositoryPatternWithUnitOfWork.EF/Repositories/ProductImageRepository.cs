// ========================================
// ProductImageRepository Implementation
// ========================================
using Microsoft.EntityFrameworkCore;
using Products.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class ProductImageRepository : BaseRepository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(ECommeeceDbContext dbcontext) : base(dbcontext)
        {
        }


        // Get all images for a specific product
        public async Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(int productId)
        {
            return await _dbContext.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.Id) // ✅ Optional: Order by ID
                .ToListAsync();
        }


        // Get a specific image by product ID and image ID
        public async Task<ProductImage?> GetImageByProductAndIdAsync(int productId, int imageId)
        {
            return await _dbContext.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.Id == imageId);
        }

        //Delete all images for a specific product
        public async Task DeleteAllProductImagesAsync(int productId)
        {
            var images = await _dbContext.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            _dbContext.ProductImages.RemoveRange(images);
        }

        // Check if product has any images
        public async Task<bool> ProductHasImagesAsync(int productId)
        {
            return await _dbContext.ProductImages
                .AnyAsync(pi => pi.ProductId == productId);
        }

        // Get count of images for a product
        public async Task<int> GetImagesCountAsync(int productId)
        {
            return await _dbContext.ProductImages
                .CountAsync(pi => pi.ProductId == productId);
        }
    }
}