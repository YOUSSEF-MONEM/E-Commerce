using Products.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    // ========================================
    // IProductImageRepository Interface
    // ========================================
    public interface IProductImageRepository : IBaseRepository<ProductImage>
    {

        // Get all images for a specific product
        Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(int productId);

  
        // Get a specific image by product ID and image ID
        Task<ProductImage?> GetImageByProductAndIdAsync(int productId, int imageId);


        // Delete all images for a specific product
        Task DeleteAllProductImagesAsync(int productId);


        // Check if product has any images
        Task<bool> ProductHasImagesAsync(int productId);


        // Get count of images for a product
        Task<int> GetImagesCountAsync(int productId);
    }
}
