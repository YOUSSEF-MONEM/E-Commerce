using Orders.Constants;
using Orders.DTOs;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Orders.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public string ShippingAddress { get; private set; } = string.Empty;
        public DateTime OrderDate { get; private set; }
        public OrderStatuses OrderStatus { get; private set; }
        public decimal TotalAmount
        {
            get
            {
                return OrderItems?.Sum(i => i.LineTotal) ?? 0;
            }
        }

        public int UserId { get; private set; }

        //  Navigation Properties
        public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
        public Payment? Payment { get; private set; } // ✅ علاقة One-to-One مع Payment

        //  Constructor خاص للـ EF Core
        private Order()
        {
            OrderDate = DateTime.UtcNow;
            OrderStatus = OrderStatuses.Pending;
        }
        //  Factory Method
        public static Result<Order> Create(
            int userId,
            string shippingAddress)
        {
            var order = new Order();

            // Validate UserId
            if (userId <= 0)
                return Result<Order>.Failure("Invalid User ID");

            // Validate ShippingAddress
            var addressResult = order.SetShippingAddress(shippingAddress);
            if (!addressResult.IsSuccess)
                return Result<Order>.Failure(addressResult.Error);

            // Set properties
            order.UserId = userId;

            return Result<Order>.Success(order);
        }

        public Result<OrderItem> AddProduct(int productId, int quantity, decimal unitPrice)
        {
            //  تحقق من حالة الأوردر
            if (OrderStatus != OrderStatuses.Pending)
                return Result<OrderItem>.Failure("Cannot add products to this order");

            //  منع تكرار المنتج
            var existingItem = OrderItems.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                var updateResult = existingItem.UpdateQuantity(existingItem.Quantity + quantity);
                if (!updateResult.IsSuccess)
                    return Result<OrderItem>.Failure(updateResult.Error);

                return Result<OrderItem>.Success(existingItem);
            }

            //  إنشاء OrderItem
            var createResult = OrderItem.Create(
                Id,
                productId,
                quantity,
                unitPrice
            );

            if (!createResult.IsSuccess)
                return Result<OrderItem>.Failure(createResult.Error);

            //  إضافة للـ Aggregate
            OrderItems.Add(createResult.Value!);

            return Result<OrderItem>.Success(createResult.Value!);
        }

        public Result RemoveOrderItem(int productId)
        {
            //  Rule: لا تعديل إلا في Pending
            if (OrderStatus != OrderStatuses.Pending)
                return Result.Failure("Cannot modify order that is not in Pending status");

            var item = OrderItems.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
                return Result.Failure("Product not found in order");

            OrderItems.Remove(item);

            return Result.Success();
        }


        //  Set Shipping Address
        public Result SetShippingAddress(string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(shippingAddress))
                return Result.Failure("Shipping address is required");

            if (shippingAddress.Length < 10)
                return Result.Failure("Shipping address must be at least 10 characters");

            if (shippingAddress.Length > 500)
                return Result.Failure("Shipping address must not exceed 500 characters");

            ShippingAddress = shippingAddress.Trim();
            return Result.Success();
        }

        //  Set Order Date
        public Result SetOrderDate(DateTime orderDate)
        {
            if (orderDate > DateTime.UtcNow)
                return Result.Failure("Order date cannot be in the future");

            OrderDate = orderDate;
            return Result.Success();
        }

        //  Set Order Status
        public Result SetOrderStatus(OrderStatuses status)
        {
            if (!Enum.IsDefined(typeof(OrderStatuses), status))
                return Result.Failure("Invalid order status");

            OrderStatus = status;
            return Result.Success();
        }

        //  Attach Payment to Order
        public Result AttachPayment(Payment payment)
        {
            if (Payment != null)
                return Result.Failure("Order already has payment attached");

            if (payment.OrderId != Id)
                return Result.Failure("Payment belongs to different order");

            Payment = payment;

            //  لما الدفع يتأكد، نحدث الأوردر
            if (payment.PaymentStatus == PaymentStatuses.Paid && OrderStatus == OrderStatuses.Pending)
            {
                OrderStatus = OrderStatuses.Processing;
            }

            return Result.Success();
        }

        //  Ship Order
        public Result ShipOrder()
        {
            //  Check payment if required (حسب متطلبات المشروع)
            // if (Payment?.PaymentStatus != PaymentStatuses.Paid)
            //     return Result.Failure("Payment must be confirmed before shipping");

            if (OrderStatus == OrderStatuses.Cancelled)
                return Result.Failure("Cannot ship cancelled order");

            if (OrderStatus == OrderStatuses.Shipped || OrderStatus == OrderStatuses.Delivered)
                return Result.Failure("Order already shipped");

            OrderStatus = OrderStatuses.Shipped;
            return Result.Success();
        }

        //  Deliver Order
        public Result DeliverOrder()
        {
            if (OrderStatus != OrderStatuses.Shipped)
                return Result.Failure("Order must be shipped before delivery");

            // Check payment (حسب متطلبات المشروع - دفع عند الاستلام أو قبل)
            if (Payment?.PaymentStatus != PaymentStatuses.Paid)
                return Result.Failure("Payment must be confirmed before delivery");

            OrderStatus = OrderStatuses.Delivered;
            return Result.Success();
        }

        //  Cancel Order
        public Result CancelOrder()
        {
            if (OrderStatus == OrderStatuses.Delivered)
                return Result.Failure("Cannot cancel delivered order");

            if (OrderStatus == OrderStatuses.Cancelled)
                return Result.Failure("Order already cancelled");

            OrderStatus = OrderStatuses.Cancelled;

            // لو كان مدفوع، نطلب Refund من Payment
            if (Payment?.PaymentStatus == PaymentStatuses.Paid)
            {
                var refundResult = Payment.RequestRefund();
                if (!refundResult.IsSuccess)
                    return refundResult;
            }

            return Result.Success();
        }

        //  Helper Methods
        public bool IsPaid() => Payment?.PaymentStatus == PaymentStatuses.Paid;

        public bool IsDelivered() => OrderStatus == OrderStatuses.Delivered;

        public bool IsCancelled() => OrderStatus == OrderStatuses.Cancelled;

        public bool CanBeCancelled() =>
            OrderStatus != OrderStatuses.Delivered &&
            OrderStatus != OrderStatuses.Cancelled;

        public bool HasPaymentInfo() => Payment != null;

        public int GetTotalItems() => OrderItems.Sum(i => i.Quantity);
    }
}





//الاتين محتاجين تحقق من حالة الاردر

// ✅ Remove Order Item
//public Result RemoveOrderItem(int productId)
//{
//    var item = OrderItems.FirstOrDefault(i => i.ProductId == productId);
//    if (item == null)
//        return Result.Failure("Item not found in order");

//    OrderItems.Remove(item);
//    return Result.Success();
//}

// ✅ Update Item Quantity

//public Result UpdateItemQuantity(int productId, int quantity)
//{
//    var item = OrderItems.FirstOrDefault(i => i.ProductId == productId);
//    if (item == null)
//        return Result.Failure("Item not found in order");

//    var result = item.UpdateQuantity(quantity);
//    if (!result.IsSuccess)
//        return result;

//    return Result.Success();
//}