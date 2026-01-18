using Result_Pattern;
using System;

namespace Carts.Entities
{
    public class CartProduct
    {
        public int CartId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }


        public DateTime AddedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        //  Computed Property
        // هسيبها تتحسب في الرن تايم احسن للادا ومش هضغط على الداتا بيز ولو عايز اجيبها بالداتا بيز جملة سليكت واجيب ضرب العمودين في عمود هحط الجمله في فيو او احفظها واستدعيها في فانكشن هنا عشان اسيت القيمه ثم استدعي الفانكشن في الكنترولر

        public decimal LineTotal
        {
            get
            {

                return Quantity * UnitPrice;

            }
        }


        //  Navigation Properties
        public Cart Cart { get; private set; } = null!;

        // ✅ Constructor خاص للـ EF Core
        private CartProduct()
        {
        }

        // ✅ Factory Method
        public static Result<CartProduct> Create(
            int cartId,
            int productId,
            int quantity,
            decimal unitPrice)
        {
            if (cartId <= 0)
                return Result<CartProduct>.Failure("Invalid Cart ID");

            if (productId <= 0)
                return Result<CartProduct>.Failure("Invalid Product ID");

            if (quantity <= 0)
                return Result<CartProduct>.Failure("Quantity must be greater than zero");

            if (unitPrice <= 0)
                return Result<CartProduct>.Failure("Unit price must be greater than zero");

            var cartProduct = new CartProduct
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                AddedAt = DateTime.UtcNow
            };

            return Result<CartProduct>.Success(cartProduct);
        }

        public static Result<CartProduct> Update(CartProduct UpdatingCartProduct, int newQuantity)
        {
            var updateQuantity = UpdatingCartProduct.UpdateQuantity(newQuantity);
            if (!updateQuantity.IsSuccess)
                return Result<CartProduct>.Failure(updateQuantity.Error);


            return Result<CartProduct>.Success(UpdatingCartProduct);
        }


        // ✅ Update Quantity
        public Result UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity must be greater than zero");

            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }


    }
}


