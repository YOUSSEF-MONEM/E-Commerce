using Carts.DTOs;
using Carts.Entities;
using Categories.Entities;
using E_Commerce2.Authorization;
using EnumsNET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class CartsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartsController> _logger;

        public CartsController(IUnitOfWork unitOfWork, ILogger<CartsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ========================================
        // Cart Operations
        // ========================================

        // Get cart by ID
        [HttpGet("mine")]//ده عشان اليوزر يجيب السله بتاعته اول ما يضغط على ايقونه السلهتيجيله بالمنتجات الاخيره اللي كان ضيفها فيها
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetMyCart()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);

            if (cart == null)
                return NotFound(new { message = "Cart not found" });

            return Ok(new
            {
                cart.Id,
                cart.UserId,
                cart.CreatedAt,
                cart.UpdatedAt,
                products =cart.CartProducts.Select(cp => new
                          {
                             cp.ProductId,
           //هنا ممكن نجيب اسم المنتج من جدول المنتجات لو حبينا
                             cp.Quantity,
                             cp.UnitPrice,
                             cp.LineTotal,
                             cp.AddedAt,
                             cp.UpdatedAt
                          }),
                totalItems = cart.GetTotalItems(),
                totalAmount = cart.GetTotalAmount(),
                isEmpty = cart.IsEmpty()
            });
        }


        // Get all carts (with no pagination)
        [HttpGet] // ده عشان التحليل والمعرفة والدراسه ونعرف كام شخص ضاف منتجات للسله ومش غمل اوردر وايه المنتجات اللي اتضافت وكده
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await _unitOfWork.Carts.GetAllAsync();

            var cartsDto = carts.Select(c => new
            {
                c!.Id,
                c.UserId,
                c.CreatedAt,
                totalItems = c.GetTotalItems(),
                totalAmount = c.GetTotalAmount()
            });

            return Ok(cartsDto);
        }


        // Delete cart
        [HttpDelete("{id}")]
        //[ValidateAntiForgeryToken]
        [CheckPermission(Roles.Manegar)]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(id);

            if (cart == null)
                return NotFound(new { message = "Cart not found" });

            await _unitOfWork.Carts.DeleteAsync(cart);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Cart deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting cart");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // Clear all items from cart
        [HttpPost("{cartId}/clear")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> ClearCart(int cartId)
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
            
            if (cart == null)
                return NotFound(new { message = "Cart not found" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (cart.UserId != userId)
                return Forbid();

            var clearResult = cart.Clear();
            if (!clearResult.IsSuccess)
                return BadRequest(new { message = clearResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while clearing cart");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ========================================
        // Cart Product Operations
        // ========================================

        // Add product to cart
        [HttpPost("{cartId}/products")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> AddProductToCart(
            int cartId,
            [FromBody] AddProductToCartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //  Validate Cart
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cart == null)
                return NotFound(new { message = "Cart not found" });


            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (cart.UserId != userId)
                return Forbid();

            //  Validate Product (await properly!)
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            //  Check Stock
            if (product.QuantityInStock < dto.Quantity)
                return BadRequest(new
                {
                    message = "Insufficient stock",
                    available = product.QuantityInStock,
                    requested = dto.Quantity
                });

            //  Add to Cart (using Cart method!)
            var addResult = cart.AddProduct(dto.ProductId, dto.Quantity, product.Price);
            if (!addResult.IsSuccess)
                return BadRequest(new { message = addResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();

                var cartProduct = addResult.Value;
                return Ok(new
                {
                    message = "Product added to cart successfully",
                    cartProduct = new
                    {
                        cartProduct!.CartId,
                        cartProduct.ProductId,
                        cartProduct.Quantity,
                        cartProduct.UnitPrice,
                        cartProduct.LineTotal
                    }
                });
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyError(ex))
            {
                return BadRequest(new { message = "Product already in cart" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding product to cart");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }


        //Update product quantity in cart
        [HttpPut("{cartId}/products/{productId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> UpdateProductQuantity(
            int cartId,
            int productId,
            [FromBody] UpdateQuantityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //  Validate Cart
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cart == null)
                return NotFound(new { message = "Cart not found" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (cart.UserId != userId)
                return Forbid();

            //  Check Stock
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (product.QuantityInStock < dto.Quantity)
                return BadRequest(new
                {
                    message = "Insufficient stock",
                    available = product.QuantityInStock
                });

            //  Update Quantity
            var updateResult = cart.UpdateProductQuantity(productId, dto.Quantity);
            if (!updateResult.IsSuccess)
                return BadRequest(new { message = updateResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Quantity updated successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating quantity");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }


        // Remove product from cart
        [HttpDelete("{cartId}/products/{productId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> RemoveProductFromCart(int cartId, int productId)
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cart == null)
                return NotFound(new { message = "Cart not found" });


            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (cart.UserId != userId)
                return Forbid();

            var removeResult = cart.RemoveProduct(productId);
            if (!removeResult.IsSuccess)
                return BadRequest(new { message = removeResult.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Product removed from cart successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while removing product");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        //تحويل اللي في العربه حاليا الي اردر ثم حذف كل العناصر اللي في العربه
        [HttpPost("place-order")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> PlaceOrder(OrderAddressDto orderAddressDto)//هيكون فيه حقل لعنوان الشحن

        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // جلب عربة المستخدم الحالية
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null || cart.IsEmpty())
                return BadRequest(new { message = "Your cart is empty" });

            // إنشاء Order جديد
            var createOrderResult = Order.Create(userId, orderAddressDto.ShippingAddress);
            if (!createOrderResult.IsSuccess)
                return BadRequest(new { message = createOrderResult.Error });

            var order = createOrderResult.Value!;

            // نسخ كل المنتجات من العربة إلى OrderItems
            foreach (var cartProduct in cart.CartProducts)
            {
                var addOrderItemResult = order.AddProduct(
                    cartProduct.ProductId,
                    cartProduct.Quantity,
                    cartProduct.UnitPrice
                );

                if (!addOrderItemResult.IsSuccess)
                    return BadRequest(new { message = addOrderItemResult.Error });
            }

            // إضافة الـ Order إلى الـ UnitOfWork
            await _unitOfWork.Orders.AddAsync(order);

            // إفراغ العربة بعد تحويلها إلى Order
            cart.Clear();

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new
                {
                    message = "Order placed successfully",
                    orderId = order.Id,
                    totalItems = order.GetTotalItems(),
                    totalAmount = order.TotalAmount
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while placing order");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }



        // ========================================
        // Helper Methods
        // ========================================

        private bool IsDuplicateKeyError(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
