using E_Commerce2.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Constants;
using Orders.DTOs;
using Orders.Entities;
using RepositoryPatternWithUnitOfWork.Core;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System.Security.Claims;
using Users.Constants;

namespace E_Commerce2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IUnitOfWork unitOfWork, ILogger<PaymentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ========================================
        // Payment CRUD Operations
        // ========================================

        // Get payment by ID
        [HttpGet("{paymentId}")]
        [CheckPermission(Roles.User)]
        //[CheckPermission(Roles.)]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (payment.UserId != userId)
                return Forbid();

            return Ok(new
            {
                payment.Id,
                payment.OrderId,
                payment.UserId,
                payment.Amount,
                payment.PaymentDate,
                paymentStatus = payment.PaymentStatus.ToString(),
                paymentMethod = payment.PaymentMethod?.ToString(),
                payment.TransactionId
            });
        }

        // Get payment by order ID
        [HttpGet("order/{orderId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            var payment = await _unitOfWork.Payments.GetPaymentByOrderIdAsync(orderId);

            if (payment == null)
                return NotFound(new { message = "No payment found for this order" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (payment.UserId != userId)
                return Forbid();

            return Ok(new
            {
                payment.Id,
                payment.OrderId,
                payment.UserId,
                payment.Amount,
                payment.PaymentDate,
                paymentStatus = payment.PaymentStatus.ToString(),
                paymentMethod = payment.PaymentMethod?.ToString(),
                payment.TransactionId
            });
        }

        // Get all payments by user ID
        [HttpGet("user")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetUserPayments( )
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var payments = await _unitOfWork.Payments.GetPaymentsByUserIdAsync(userId);

            var paymentsDto = payments.Select(p => new
            {
                p.Id,
                p.OrderId,
                p.Amount,
                p.PaymentDate,
                paymentStatus = p.PaymentStatus.ToString(),
                paymentMethod = p.PaymentMethod?.ToString(),
                p.TransactionId
            });

            return Ok(paymentsDto);
        }

        // Get all payments 
        [HttpGet]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();

            var paymentsDto = payments.Select(p => new
            {
                p!.Id,
                p.OrderId,
                p.UserId,
                p.Amount,
                p.PaymentDate,
                paymentStatus = p.PaymentStatus.ToString(),
                paymentMethod = p.PaymentMethod?.ToString(),
                p.TransactionId
            });

            return Ok(paymentsDto);
        }

        // ========================================
        // Payment Operations
        // ========================================

        //  
        [HttpPost("initiate")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (order.UserId != userId)
                return StatusCode(403, new { message = "This Order is not for you" }); //Forbid

            // ✅ Check if payment already exists
            var existingPayment = await _unitOfWork.Payments.GetPaymentByOrderIdAsync(dto.OrderId);
            if (existingPayment != null)
                return BadRequest(new { message = "Payment already exists for this order" });

            // ✅ Check order status
            if (order.OrderStatus == OrderStatuses.Cancelled)
                return BadRequest(new { message = "Cannot create payment for cancelled order" });

            // ✅ Create Payment
            var paymentResult = Payment.Create(
                order.Id,
                order.UserId,
                order.TotalAmount
            );

            if (!paymentResult.IsSuccess)
                return BadRequest(new { message = paymentResult.Error });

            var payment = paymentResult.Value!;

            // ✅ Attach Payment to Order
            var attachResult = order.AttachPayment(payment);
            if (!attachResult.IsSuccess)
                return BadRequest(new { message = attachResult.Error });

            await _unitOfWork.Payments.AddAsync(payment);

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetPaymentById),
                    new { id = payment.Id },
                    new
                    {
                        message = "Payment initiated successfully",
                        paymentId = payment.Id,
                        orderId = payment.OrderId,
                        amount = payment.Amount,
                        paymentStatus = payment.PaymentStatus.ToString()
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while initiating payment");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // Confirm payment
        [HttpPost("{paymentId}/confirm")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> ConfirmPayment(
            int paymentId,
            [FromBody] ConfirmPaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (payment.UserId != userId)
                return StatusCode(403, new { message = "This payment process is not for you" }); //Forbid
            

            var confirmResult = payment.ConfirmPayment(dto.TransactionId, dto.PaymentMethod);
            if (!confirmResult.IsSuccess)
                return BadRequest(new { message = confirmResult.Error });

            // ✅ Update Order Status if needed
            var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
            if (order != null && order.OrderStatus == OrderStatuses.Pending)
            {
                var statusResult = order.SetOrderStatus(OrderStatuses.Processing);
                if (!statusResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to update order status: {Error}", statusResult.Error);
                }
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    message = "Payment confirmed successfully",
                    paymentId = payment.Id,
                    orderId = payment.OrderId,
                    paymentStatus = payment.PaymentStatus.ToString(),
                    paymentMethod = payment.PaymentMethod?.ToString(),
                    paymentDate = payment.PaymentDate,
                    transactionId = payment.TransactionId
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while confirming payment");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // Mark payment as failed
        //مش عارف هستعملهل ليه؟
        //دول محدش بيتحكم فيهم دول تبع الجت اوي 
        //ده مش API يوزر، ده API سيستم / Gateway


        //[HttpPost("{paymentId}/mark-failed")]
        //public async Task<IActionResult> MarkPaymentFailed(int paymentId)
        //{
        //    var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
        //    if (payment == null)
        //        return NotFound(new { message = "Payment not found" });

        //    var failResult = payment.MarkPaymentFailed();
        //    if (!failResult.IsSuccess)
        //        return BadRequest(new { message = failResult.Error });

        //    try
        //    {
        //        await _unitOfWork.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            message = "Payment marked as failed",
        //            paymentId = payment.Id,
        //            orderId = payment.OrderId,
        //            paymentStatus = payment.PaymentStatus.ToString()
        //        });
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        _logger.LogError(ex, "Database error");
        //        return StatusCode(500, new { message = "Database error occurred" });
        //    }
        //}

        // Request refund
        //[HttpPost("{paymentId}/refund")]
        //public async Task<IActionResult> RequestRefund(int paymentId)
        //{
        //    var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
        //    if (payment == null)
        //        return NotFound(new { message = "Payment not found" });

        //    var refundResult = payment.RequestRefund();
        //    if (!refundResult.IsSuccess)
        //        return BadRequest(new { message = refundResult.Error });

        //    try
        //    {
        //        await _unitOfWork.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            message = "Refund processed successfully",
        //            paymentId = payment.Id,
        //            orderId = payment.OrderId,
        //            paymentStatus = payment.PaymentStatus.ToString()
        //        });
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        _logger.LogError(ex, "Database error while processing refund");
        //        return StatusCode(500, new { message = "Database error occurred" });
        //    }
        //}

        // Update payment amount (before confirmation)
        //[HttpPut("{id}/amount")]
        //public async Task<IActionResult> UpdatePaymentAmount(
        //    int id,
        //    [FromBody] decimal newAmount)
        //{
        //    if (newAmount <= 0)
        //        return BadRequest(new { message = "Amount must be greater than zero" });

        //    var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        //    if (payment == null)
        //        return NotFound(new { message = "Payment not found" });

        //    var updateResult = payment.UpdateAmount(newAmount);
        //    if (!updateResult.IsSuccess)
        //        return BadRequest(new { message = updateResult.Error });

        //    try
        //    {
        //        await _unitOfWork.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            message = "Payment amount updated successfully",
        //            paymentId = payment.Id,
        //            newAmount = payment.Amount
        //        });
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        _logger.LogError(ex, "Database error while updating payment amount");
        //        return StatusCode(500, new { message = "Database error occurred" });
        //    }
        //}

        // Delete payment (Admin only - for pending payments)
        [HttpDelete("{paymentId}")]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            // ✅ Only allow deleting pending or failed payments
            if (payment.PaymentStatus == PaymentStatuses.Paid ||
                payment.PaymentStatus == PaymentStatuses.Refunded)
            {
                return BadRequest(new
                {
                    message = "Cannot delete paid or refunded payments"
                });
            }

            await _unitOfWork.Payments.DeleteAsync(payment);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Payment deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting payment");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ========================================
        // Payment Statistics (Admin)
        // ========================================

        // Get payment statistics by status
        [HttpGet("statistics/by-status")]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> GetPaymentStatisticsByStatus()
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();

            var statistics = payments
                .GroupBy(p => p!.PaymentStatus)
                .Select(g => new
                {
                    status = g.Key.ToString(),
                    count = g.Count(),
                    totalAmount = g.Sum(p => p!.Amount)
                });

            return Ok(statistics);
        }

        // Get payment statistics by method
        [HttpGet("statistics/by-method")]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> GetPaymentStatisticsByMethod()
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();

            var statistics = payments
                .Where(p => p!.PaymentMethod.HasValue)
                .GroupBy(p => p!.PaymentMethod!.Value)
                .Select(g => new
                {
                    method = g.Key.ToString(),
                    count = g.Count(),
                    totalAmount = g.Sum(p => p!.Amount)
                });

            return Ok(statistics);
        }
    }
}