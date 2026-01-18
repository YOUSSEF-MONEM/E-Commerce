using Orders.Constants;
using Result_Pattern;
using System;

namespace Orders.Entities
{
    public class Payment
    {
        public int Id { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime? PaymentDate { get; private set; }
        public PaymentStatuses PaymentStatus { get; private set; }
        public PaymentMethods? PaymentMethod { get; private set; }
        public string? TransactionId { get; private set; } // ✅ معرف المعاملة من Payment Gateway

        // ✅ Foreign Keys
        public int OrderId { get; private set; }
        public int UserId { get; private set; }

        // ✅ Navigation Property
        public Order? Order { get; private set; }

        // ✅ Constructor خاص للـ EF Core
        private Payment()
        {
            PaymentStatus = PaymentStatuses.Pending;
        }

        // ✅ Factory Method
        public static Result<Payment> Create(
            int orderId,
            int userId,
            decimal amount)
        {
            var payment = new Payment();

            if (orderId <= 0)
                return Result<Payment>.Failure("Invalid Order ID");

            if (userId <= 0)
                return Result<Payment>.Failure("Invalid User ID");

            if (amount <= 0)
                return Result<Payment>.Failure("Amount must be greater than zero");

            payment.OrderId = orderId;
            payment.UserId = userId;
            payment.Amount = amount;

            return Result<Payment>.Success(payment);
        }

        //  Confirm Payment
        public Result ConfirmPayment(string transactionId, PaymentMethods paymentMethod)
        {
            if (PaymentStatus == PaymentStatuses.Paid)
                return Result.Failure("Payment already confirmed");

            if (PaymentStatus == PaymentStatuses.Refunded)
                return Result.Failure("Cannot confirm refunded payment");

            if (string.IsNullOrWhiteSpace(transactionId))
                return Result.Failure("Transaction ID is required");

            if (!Enum.IsDefined(typeof(PaymentMethods), paymentMethod))
                return Result.Failure("Invalid payment method");

            // ✅ Set payment fields
            TransactionId = transactionId.Trim();
            PaymentMethod = paymentMethod;
            PaymentDate = DateTime.UtcNow;
            PaymentStatus = PaymentStatuses.Paid;

            return Result.Success();
        }

        // ✅ Mark Payment as Failed
        public Result MarkPaymentFailed()
        {
            if (PaymentStatus == PaymentStatuses.Paid)
                return Result.Failure("Cannot mark paid payment as failed");

            if (PaymentStatus == PaymentStatuses.Refunded)
                return Result.Failure("Cannot mark refunded payment as failed");

            PaymentStatus = PaymentStatuses.Failed;
            return Result.Success();
        }

        // ✅ Request Refund
        public Result RequestRefund()
        {
            if (PaymentStatus != PaymentStatuses.Paid)
                return Result.Failure("Only paid payments can be refunded");

            if (PaymentStatus == PaymentStatuses.Refunded)
                return Result.Failure("Payment already refunded");

            PaymentStatus = PaymentStatuses.Refunded;
            return Result.Success();
        }

        // ✅ Update Amount (قبل الدفع فقط)
        public Result UpdateAmount(decimal newAmount)
        {
            if (PaymentStatus == PaymentStatuses.Paid)
                return Result.Failure("Cannot update amount for paid payment");

            if (PaymentStatus == PaymentStatuses.Refunded)
                return Result.Failure("Cannot update amount for refunded payment");

            if (newAmount <= 0)
                return Result.Failure("Amount must be greater than zero");

            Amount = newAmount;
            return Result.Success();
        }

        // ✅ Helper Methods
        public bool IsPaid() => PaymentStatus == PaymentStatuses.Paid;

        public bool IsPending() => PaymentStatus == PaymentStatuses.Pending;

        public bool IsFailed() => PaymentStatus == PaymentStatuses.Failed;

        public bool IsRefunded() => PaymentStatus == PaymentStatuses.Refunded;

        public bool CanBeModified() =>
            PaymentStatus != PaymentStatuses.Paid &&
            PaymentStatus != PaymentStatuses.Refunded;
    }
}