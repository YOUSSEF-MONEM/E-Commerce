using System;
using Products.DTOs;
using Result_Pattern;

namespace Products.Entities
{
    public class Product
    {
        public int Id { get; private set; }
        public  string ProductName { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public double DiscountPercentage { get; set; } 
        public int? QuantityInStock { get; private set; }
        public string ProductDescription { get; private set; } = string.Empty;
        public int CategoryId { get; private set; }
        //addColumnSellerIdToProductsTable
        public int SellerId { get; private set; } // للبائع فقط 
        public ICollection<ProductImage> ProductImages { get; private set; } = new List<ProductImage>();
        //public User user { get; private set; }
        // Constructor خاص للـ EF Core
        private Product() { }

        // Factory Method بدل Constructor العام
        public static Result<Product> Create(
            string productName,
            decimal price,
            double discountPercentage,
            int quantityInStock,
            string productDescription,
            int categoryId,
            int sellerId)
        {
            var product = new Product();

            var nameResult = product.SetProductName(productName);
            if (!nameResult.IsSuccess)
                return Result<Product>.Failure(nameResult.Error ?? string.Empty);

            var priceResult = product.SetProductPrice(price);
            if (!priceResult.IsSuccess)
                return Result<Product>.Failure(priceResult.Error ?? string.Empty);

            var discountPercentageResult = product.SetDiscountPercentage(discountPercentage);
            if (!discountPercentageResult.IsSuccess)
                return Result<Product>.Failure(discountPercentageResult.Error ?? string.Empty);

            var quantityResult = product.SetQuantityInStock(quantityInStock);
            if (!quantityResult.IsSuccess)
                return Result<Product>.Failure(quantityResult.Error ?? string.Empty);

     
            var descriptionResult = product.SetProductDescription(productDescription);
            if (!descriptionResult.IsSuccess)
                return Result<Product>.Failure(descriptionResult.Error ?? string.Empty);

            var categoryResult = product.SetCategoryId(categoryId);
            if (!categoryResult.IsSuccess)
                return Result<Product>.Failure(categoryResult.Error ?? string.Empty);

            var salerIdResult = product.SetSalerId(sellerId);
            if (!salerIdResult.IsSuccess)
                return Result<Product>.Failure(salerIdResult.Error ?? string.Empty);

            return Result<Product>.Success(product);
        }

        public static Result<Product> Update(Product product,UpdateProductDto updateProductDto)
        {

            if (!string.IsNullOrWhiteSpace(updateProductDto.ProductName))
            {
                var result = product.SetProductName(updateProductDto.ProductName);
                if (!result.IsSuccess)
                    return Result<Product>.Failure(result.Error);
            }

            if (updateProductDto.Price.HasValue)
            {
                var result = product.SetProductPrice(updateProductDto.Price.Value);
                if (!result.IsSuccess)
                    return Result<Product>.Failure(result.Error);
            }

            if (updateProductDto.DiscountPercentage.HasValue)
            {
                var result = product.SetDiscountPercentage(updateProductDto.DiscountPercentage.Value);
                if (!result.IsSuccess)
                    return Result<Product>.Failure(result.Error);
            }

            if (updateProductDto.QuantityInStock.HasValue)
            {
                var result = product.SetQuantityInStock(updateProductDto.QuantityInStock);
                if (!result.IsSuccess)
                    return Result<Product>.Failure(result.Error);
            }

            //if (!string.IsNullOrWhiteSpace(updateProductDto.ProductImageURL))
            //{
            //    var result = product.SetProductImageURL(updateProductDto.ProductImageURL);
            //    if (!result.IsSuccess)
            //        return Result<Product>.Failure(result.Error);
            //}

            if (!string.IsNullOrWhiteSpace(updateProductDto.ProductDescription))
            {
                var result = product.SetProductDescription(updateProductDto.ProductDescription);
                if (!result.IsSuccess)
                    return Result<Product>.Failure(result.Error);
            }

            if(updateProductDto.CategoryId.HasValue)
            {
                var result = product.SetCategoryId(updateProductDto.CategoryId.Value);
                if (!result.IsSuccess)
                    return Result<Product>.Failure(result.Error);
            }

            return Result<Product>.Success(product);

        }

        // SetProductName بدون Exception
        public Result SetProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return Result.Failure("Product Name is required");

            if (productName.Length < 3)
                return Result.Failure("Product Name must be at least 3 characters");

            if (productName.Length > 100)
                return Result.Failure("Product Name must not exceed 100 characters");

            ProductName = productName.Trim();
            return Result.Success();
        }

        //  SetProductPrice بدون Exception
        public Result SetProductPrice(decimal price)
        {
            if (price <= 0)
                return Result.Failure("Price must be greater than zero");

            if (price > 1000000)
                return Result.Failure("Price is too high");

            Price = price;
            return Result.Success();
        }
        //  SetDiscountPercentage بدون Exception
        public Result SetDiscountPercentage(double discountPercentage)
        {
            if (discountPercentage > 0.15)
                return Result.Failure("DiscountPrice is too high");

            DiscountPercentage = discountPercentage;
            return Result.Success();
        }

        // SetQuantityInStock بدون Exception
        public Result SetQuantityInStock(int? quantityInStock)
        {
            if (quantityInStock < 0)
                return Result.Failure("Quantity in stock cannot be negative");

            QuantityInStock = quantityInStock;
            return Result.Success();
        }


        // SetProductDescription بدون Exception
        public Result SetProductDescription(string productDescription)
        {
            if (string.IsNullOrWhiteSpace(productDescription))
                return Result.Failure("Product Description is required");

            if (productDescription.Length < 10)
                return Result.Failure("Product Description must be at least 10 characters");

            if (productDescription.Length > 1000)
                return Result.Failure("Product Description must not exceed 1000 characters");

            ProductDescription = productDescription.Trim();
            return Result.Success();
        }

        // SetCategoryId بدون Exception
        public Result SetCategoryId(int categoryId)
        {
            if (categoryId <= 0)
                return Result.Failure("Category ID must be greater than zero");

            CategoryId = categoryId;
            return Result.Success();
        }

        public Result SetSalerId(int sellerId)
        {
            if (sellerId <= 0)
                return Result.Failure("Saler ID must be greater than zero");
            SellerId = sellerId;
            return Result.Success();
        }

        // Update Quantity (زيادة أو نقصان)
        public Result AddStock(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity to add must be greater than zero");

            QuantityInStock += quantity;
            return Result.Success();
        }

        public Result RemoveStock(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity to remove must be greater than zero");

            if (QuantityInStock < quantity)
                return Result.Failure($"Insufficient stock. Available: {QuantityInStock}");

            QuantityInStock -= quantity;
            return Result.Success();
        }

        // Check if product is in stock
        public bool IsInStock() => QuantityInStock > 0;

        // Check if enough stock available
        public bool HasEnoughStock(int requestedQuantity) => QuantityInStock >= requestedQuantity;
    }
}
