using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Carts.Entities
{
    public class Cart
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        //  Navigation Properties
       
        public ICollection<CartProduct> CartProducts { get; private set; } = new List<CartProduct>();

        //  Constructor خاص للـ EF Core
        private Cart()
        { 
            CreatedAt = DateTime.UtcNow;
        }

        //  Factory Method
        public static Result<Cart> Create(int userId)
        {
            if (userId <= 0)
                return Result<Cart>.Failure("Invalid User ID");

            var cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            return Result<Cart>.Success(cart);
        }

        public static Result<Cart> Update(Cart cartUpdating, int newUserId)
        {
            if (newUserId <= 0)
                return Result<Cart>.Failure("Invalid User ID");
            cartUpdating.UserId = newUserId; // نتاكد من وجود اليوزر ده من ال .EF قبل ما اعمل التحديث
            cartUpdating.UpdatedAt = DateTime.UtcNow;
            return Result<Cart>.Success(cartUpdating);
        }

        //  Add Product to Cart
        public Result<CartProduct> AddProduct(int productId, int quantity, decimal unitPrice)
        {
            if (productId <= 0)
                return Result<CartProduct>.Failure("Invalid Product ID");

            if (quantity <= 0)
                return Result<CartProduct>.Failure("Quantity must be greater than zero");

            if (unitPrice <= 0)
                return Result<CartProduct>.Failure("Unit price must be greater than zero");

            // Check if product already exists
            var existingProduct = CartProducts.FirstOrDefault(cp => cp.ProductId == productId);

            if (existingProduct != null)
            {
                // Update quantity
                var updateResult = existingProduct.UpdateQuantity(existingProduct.Quantity + quantity);
                if (!updateResult.IsSuccess)
                    return Result<CartProduct>.Failure(updateResult.Error);

                MarkAsUpdated();
                return Result<CartProduct>.Success(existingProduct);
            }

            // Create new CartProduct
            var cartProductResult = CartProduct.Create(Id, productId, quantity, unitPrice);
            if (!cartProductResult.IsSuccess)
                return Result<CartProduct>.Failure(cartProductResult.Error);

            CartProducts.Add(cartProductResult.Value!);
            MarkAsUpdated();

            return Result<CartProduct>.Success(cartProductResult.Value!);
        }

        //  Remove Product from Cart
        public Result RemoveProduct(int productId)
        {
            var cartProduct = CartProducts.FirstOrDefault(cp => cp.ProductId == productId);

            if (cartProduct == null)
                return Result.Failure("Product not found in cart");

            CartProducts.Remove(cartProduct);
            MarkAsUpdated();

            return Result.Success();
        }

        //  Update Product Quantity
        public Result UpdateProductQuantity(int productId, int quantity)
        {
            var cartProduct = CartProducts.FirstOrDefault(cp => cp.ProductId == productId);

            if (cartProduct == null)
                return Result.Failure("Product not found in cart");

            var updateResult = cartProduct.UpdateQuantity(quantity);
            if (!updateResult.IsSuccess)
                return updateResult;

            MarkAsUpdated();
            return Result.Success();
        }

        //  Clear Cart
        public Result Clear()
        {
            CartProducts.Clear();
            MarkAsUpdated();
            return Result.Success();
        }

        //  Helper Methods
        public bool IsEmpty() => !CartProducts.Any();

        public int GetTotalItems() => CartProducts.Sum(cp => cp.Quantity);

        public Decimal GetTotalAmount() => CartProducts.Sum(cp => cp.LineTotal);

        private void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}