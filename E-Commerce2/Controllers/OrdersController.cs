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
using Users.Entities;

namespace E_Commerce2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IUnitOfWork unitOfWork, ILogger<OrdersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ========================================
        // Order CRUD Operations
        // ========================================

        // في حالة ال Pending معلق  قيد الانتظار وده متحقق منه في الدومين بتاع ال AddProduct
        [HttpPost("{orderId}/items")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> AddProduct(  int orderId, [FromBody] AddOrderItemDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
                return NotFound("Order not found");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (order.UserId != userId)
                return Forbid("You do not have permission to modify this order");

            var result = order.AddProduct(
                dto.ProductId,
                dto.Quantity,
                dto.UnitPrice
            );

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            await _unitOfWork.SaveChangesAsync();
            return Ok(result.Value);
        }


        // في حالة ال Pending معلق  قيد الانتظار وده متحقق منه في الدومين بتاع ال UpdateQuantity
        [HttpPut("{orderId}/items/{productId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> UpdateQuantity( int orderId, int productId, [FromBody] UpdateOrderItemQuantityDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
                return NotFound();

            var item = order.OrderItems.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
                return NotFound("Product not found in order");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (order.UserId != userId)
                return Forbid();


            var result = item.UpdateQuantity(dto.Quantity);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }

        // في حالة ال Pending معلق  قيد الانتظار وده متحقق منه في الدومين بتاع ال Remove
        [HttpDelete("{orderId}/items/{productId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> RemoveProduct( int orderId,int productId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (order.UserId != userId)
                return Forbid();


            var result = order.RemoveOrderItem(productId);
   
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("my-orders")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);

            var ordersDto = orders.Select(o => new
            {
                o.Id,
                o.OrderDate,
                orderStatus = o.OrderStatus.ToString(),
                o.ShippingAddress,
                o.OrderItems,
                o.TotalAmount,
                totalItems = o.GetTotalItems(),
                hasPayment = o.Payment != null,
                paymentStatus = o.Payment?.PaymentStatus.ToString()
            });

            return Ok(ordersDto);

        }


        //تفاصيل اردر معين من الاردارات وفيه فرصه للمستخدم يعدل عليه لو في حاله معلقه Pending 
        //بمعني لو الاردر في حالة معلق يظهر له تفاصيل الاردر مع امكانية التعديل عليه ولما يعدل 
        [HttpGet("{orderId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _unitOfWork.Orders.ViewByIdAsync(orderId);

            if (order == null || order.UserId != userId)
                return NotFound();

            var orderDto = new
            {
                order.Id,
                order.OrderDate,
                orderStatus = order.OrderStatus.ToString(),
                order.ShippingAddress,
                order.OrderItems,
                order.TotalAmount,
                totalItems = order.GetTotalItems(),
                hasPayment = order.Payment != null,
                paymentStatus = order.Payment?.PaymentStatus.ToString()
            };

            return Ok(orderDto);
        }


        // ========================================
        // Order Status Operations
        // ========================================

        // Ship an order
        [HttpPost("{id}/ship")]
        [CheckPermission(Roles.Delivery)]
        public async Task<IActionResult> ShipOrder(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            var shipResult = order.ShipOrder();
            if (!shipResult.IsSuccess)
                return BadRequest(new { message = shipResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    message = "Order shipped successfully",
                    orderId = order.Id,
                    orderStatus = order.OrderStatus.ToString()
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while shipping order");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // Deliver an order
        [HttpPost("{id}/deliver")]
        [CheckPermission(Roles.Delivery)]
        public async Task<IActionResult> DeliverOrder(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            var deliverResult = order.DeliverOrder();
            if (!deliverResult.IsSuccess)
                return BadRequest(new { message = deliverResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    message = "Order delivered successfully",
                    orderId = order.Id,
                    orderStatus = order.OrderStatus.ToString()
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while delivering order");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // Cancel an order
        [HttpPost("{id}/cancel")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Restore stock for cancelled orders
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.AddStock(item.Quantity);
                }
            }

            var cancelResult = order.CancelOrder();
            if (!cancelResult.IsSuccess)
                return BadRequest(new { message = cancelResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    message = "Order cancelled successfully",
                    orderId = order.Id,
                    orderStatus = order.OrderStatus.ToString(),
                    paymentStatus = order.Payment?.PaymentStatus.ToString()
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while cancelling order");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // Delete order (Admin only)
        [HttpDelete("{id}")]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            //  Only allow deleting pending/cancelled orders
            if (order.OrderStatus != OrderStatuses.Pending &&
                order.OrderStatus != OrderStatuses.Cancelled)
            {
                return BadRequest(new { message = "Cannot delete processed orders" });
            }

            //  Delete associated payment if exists
            if (order.Payment != null)
            {
                await _unitOfWork.Payments.DeleteAsync(order.Payment);
            }

            await _unitOfWork.Orders.DeleteAsync(order);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Order deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting order");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }
    }
}