using Result_Pattern;
using System;

namespace Orders.Entities
{
    public class OrderItem
    {
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        // Computed Property
        // هسيبها تتحسب في الرن تايم احسن للادا ومش هضغط على الداتا بيز ولو عايز اجيبها بالداتا بيز جملة سليكت واجيب ضرب العمودين في عمود هحط الجمله في فيو او احفظها واستدعيها في فانكشن هنا عشان اسيت القيمه ثم استدعي الفانكشن في الكنترولر
        public decimal LineTotal
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }

        // Navigation Properties
        public Order Order { get; private set; } = null!;

        // Constructor
        private OrderItem() { }

        // ✅ Factory Method
        public static Result<OrderItem> Create(
            int orderId,
            int productId,
            int quantity,
            decimal unitPrice)
        {
            //دي مش محتاج اتشك عليها لانها بتجنريت لوحدها من الداتا بيز
            /*
             🔴 ماينفعش الـ OrderItem يتحقق من OrderId

لأن:

الـ Order هو الـ Aggregate Root

والـ OrderItem بيعيش جواه
             */
            //if (orderId <= 0)
            //    return Result<OrderItem>.Failure("Invalid Order ID");

            if (productId <= 0)
                return Result<OrderItem>.Failure("Invalid Product ID");

            if (quantity <= 0)
                return Result<OrderItem>.Failure("Quantity must be greater than zero");

            if (unitPrice <= 0)
                return Result<OrderItem>.Failure("Unit price must be greater than zero");

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            return Result<OrderItem>.Success(orderItem);
        }

        // ✅ Update Quantity
        public Result UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity must be greater than zero");

            Quantity = quantity;
            return Result.Success();
        }

    }
}


//// ✅ Update Price الاردر ايتم ملوش علاقه بالسعر بتاع المنتج 
//public Result UpdatePrice(decimal unitPrice)
//{
//    if (unitPrice <= 0)
//        return Result.Failure("Unit price must be greater than zero");

//    UnitPrice = unitPrice;
//    return Result.Success();
//}