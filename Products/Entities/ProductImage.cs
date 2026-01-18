using Result_Pattern;
using System;

namespace Products.Entities
{
    public class ProductImage
    {
        public int Id { get; private set; }
        public string ImageURL { get; private set; } = string.Empty;
        public int ProductId { get; private set; }

        // ✅ Navigation Property
        public Product? Product { get; private set; }

        // ✅ Private Constructor for EF Core
        private ProductImage()
        {
        }

        // ✅ Static Factory Method
        public static Result<ProductImage> Create(int productId, string imageUrl)
        {
            var productImage = new ProductImage();

            // Validate ProductId
            if (productId <= 0)
                return Result<ProductImage>.Failure("Invalid Product ID");

            // Validate ImageURL
            var imageUrlResult = productImage.SetImageURL(imageUrl);
            if (!imageUrlResult.IsSuccess)
                return Result<ProductImage>.Failure(imageUrlResult.Error);

            // Set ProductId
            productImage.ProductId = productId;

            return Result<ProductImage>.Success(productImage);
        }

        // ✅ Set Image URL
        public Result SetImageURL(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return Result.Failure("Image URL is required");

            if (imageUrl.Length > 500)
                return Result.Failure("Image URL cannot exceed 500 characters");

            // ✅ Basic URL validation
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return Result.Failure("Invalid URL format");
            }

            ImageURL = imageUrl.Trim();
            return Result.Success();
        }

        // ✅ Update Image URL
        public Result UpdateImageURL(string newImageUrl)
        {
            return SetImageURL(newImageUrl);
        }
    }
}
