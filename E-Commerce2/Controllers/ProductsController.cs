using E_Commerce2.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.DTOs;
using Products.Entities;
using RepositoryPatternWithUnitOfWork.Core;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using Result_Pattern;
using System.Security.Claims;
using Users.Constants;

namespace E_Commerce2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ========================================
        // Product CRUD Operations
        // ========================================

        // ✅ POST: api/Products
        [HttpPost]
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> Create([FromBody] RegisterProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // ✅ Step 1: Create Product
            var productResult = Product.Create(
                productDto.ProductName,
                productDto.Price,
                productDto.DiscountPercentage,
                productDto.QuantityInStock,
                productDto.ProductDescription,
                productDto.CategoryId,
               sellerId
            );

            if (!productResult.IsSuccess)
                return BadRequest(new { message = productResult.Error });

            if (productResult.Value == null)
                return BadRequest(new { message = "Failed to create product" });

            var product = productResult.Value;
            await _unitOfWork.Products.AddAsync(product);

            try
            {
                // ✅ Step 2: Save Product to get ProductId
                await _unitOfWork.SaveChangesAsync();

                // ✅ Step 3: Add Product Image if provided
                if (!string.IsNullOrWhiteSpace(productDto.ProductImageURL))
                {
                    var imageResult = ProductImage.Create(product.Id, productDto.ProductImageURL);

                    if (imageResult.IsSuccess && imageResult.Value != null)
                    {
                        await _unitOfWork.ProductImages.AddAsync(imageResult.Value);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Product created but image failed: {Error}",
                            imageResult.Error
                        );
                    }
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = product.Id },
                    new
                    {
                        product.Id,
                        product.ProductName,
                        product.Price,
                        product.QuantityInStock,
                        hasImage = !string.IsNullOrWhiteSpace(productDto.ProductImageURL)
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating product");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            var productDto = new
            {
                product.Id,
                product.ProductName,
                product.Price,
                product.DiscountPercentage,
                product.QuantityInStock,
                product.ProductDescription,
                product.CategoryId,
                productImages = product.ProductImages.Select(img => new
                {
                    img.Id,
                    img.ImageURL
                })
            };
            return Ok(productDto);
        }

        // GET: api/Products
        [HttpGet()]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetAll()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var productsDto = products.Select(product => new
            {
                product!.Id,
                product.ProductName,
                product.Price,
                product.DiscountPercentage,
                product.QuantityInStock,
                product.ProductDescription,
                product.CategoryId,
                productImages = product.ProductImages.Select(img => new
                {
                    img.Id,
                    img.ImageURL
                })
            });
            return Ok(products);
        }


        [HttpGet("my")]//لما المستخدم يضغط منتجاتي اجيبهم من هنا بالايدي بتاعتهم بس مش هيعرض الفرنت هيحتفظ بيه عشان بعد كده لو البائع عايز يعدل منتج يبعته معا الركويست عشان هحتاجه لما يضغط تعديل
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> GetMyProducts() // no traking لما يجي على منتج ويعدل ويضغط تعديل دا ريكوست للابديت
        {
            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var products = await _unitOfWork.Products.GetBySellerIdAsync(sellerId);

            var productsDto = products.Select(product => new
            {
                product!.Id,
                product.ProductName,
                product.Price,
                product.DiscountPercentage,
                product.QuantityInStock,
                product.ProductDescription,
                product.CategoryId,
                productImages = product.ProductImages.Select(img => new
                {
                    img.Id,
                    img.ImageURL
                })
            });

            return Ok(productsDto);
        }

        // ✅ GET: api/Products
        [HttpGet("search")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetByName([FromQuery]string  productName)
        {
            if(string.IsNullOrWhiteSpace(productName))
                return BadRequest(new { message = "Product name must be provided" });

            var products = await _unitOfWork.Products.GetProductsByName(productName);
            if (products == null || !products.Any())
                return NotFound(new { message = $"No products found containing: {productName}" });
            var productsDto = products.Select(p => new
            {
                p.Id,
                p.ProductName,
                p.Price,
                p.DiscountPercentage,
                p.QuantityInStock,
                p.ProductDescription,
                p.CategoryId,
                productImeges = p.ProductImages.Select(img => new
                {
                    img.Id,
                    img.ImageURL
                })
            });

            return Ok(productsDto);
        }

        // ✅ PUT: api/Products
        [HttpPut("{id}")] // ال Id هيكون الفرنت اند حافظه عشان لما يضغط على تعديل المنتج هيبعته مع الريكوست 
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> Update(int id,[FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (product.SellerId != sellerId)
                return Forbid();


            var resultUpdate = Product.Update(product, updateProductDto);
            if (!resultUpdate.IsSuccess)
                return BadRequest(new { message = resultUpdate.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Product updated successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating product");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ✅ DELETE: api/Products/5
        [HttpDelete("{id}")]
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> Delete(int id)//id هيكون فيه في منتجات البائع ان كل منتج عليه زي زرار اسمو حذف ومن هنا اصلا هيكون الاي دي موجود مع الفرنت فيبعته في اليو ار ال
        {
            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (product.SellerId != sellerId)
                return Forbid();

            // ✅ Delete associated images first (if using Restrict DeleteBehavior)
            var images = await _unitOfWork.ProductImages.GetImagesByProductIdAsync(id);
            foreach (var image in images)
            {
                await _unitOfWork.ProductImages.DeleteAsync(image);
            }

            await _unitOfWork.Products.DeleteAsync(product);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting product");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ========================================
        // Product Images Operations
        // ========================================

        // ✅ POST: api/Products/5/images - Add image(s) to existing product
        [HttpPost("{productId}/images")] // طبعا ال id بتاع المنتج هيبقى مع الفرنت اند عشان لما يضغط على اضافه صوره هيبعته مع الريكوست انا كباك اند لبرجع ال id مع الريسبونس فالفرنت اند هيقدر يحتفظ بيه
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> AddProductImage(int productId,


            [FromBody] AddProductImageDto imageDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (sellerId != product.SellerId)
                return Forbid();


            if (product.ProductImages.Count >= 5)
                return BadRequest("Maximum 5 images allowed");

            var imageResult = ProductImage.Create(productId, imageDto.ImageURL);
            if (!imageResult.IsSuccess)
                return BadRequest(new { message = imageResult.Error });

            if (imageResult.Value == null)
                return BadRequest(new { message = "Failed to create image" });

            await _unitOfWork.ProductImages.AddAsync(imageResult.Value);

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetProductImages),
                    new { productId },
                    new
                    {
                        imageId = imageResult.Value.Id,
                        productId,
                        imageUrl = imageResult.Value.ImageURL,
                        message = "Image added successfully"
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding image");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ✅ POST: api/Products/5/images/multiple - Add multiple images
        //ممكن اشيل الاكشن ده لو مش هستخدمه لان انا ممكن اضيف صوره واحده واحده من خلال الاكشن اللي فوق
        [HttpPost("{productId}/images/multiple")]
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> AddMultipleProductImages(
            int productId,
            [FromBody] AddMultipleImagesDto imagesDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });


            if (sellerId != product.SellerId)
                return Forbid();

            if (product.ProductImages.Count >= 5)///////////////////////////////////////////////لو كان فيه 4 والمستخدم بيضبف 3 هيضافو فهنا دي مش تنفع مع الملتي ولكن تنفع مع اضافت صوره واحده واحده
                return BadRequest("Maximum 5 images allowed");

            var addedImages = new List<ProductImage>();

            foreach (var imageUrl in imagesDto.ImageURLs)
            {
                var imageResult = ProductImage.Create(productId, imageUrl);

                if (imageResult.IsSuccess && imageResult.Value != null)
                {
                    await _unitOfWork.ProductImages.AddAsync(imageResult.Value);
                    addedImages.Add(imageResult.Value);
                }
                else
                {
                    _logger.LogWarning("Failed to add image {Url}: {Error}", imageUrl, imageResult.Error);
                }
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{addedImages.Count} image(s) added successfully",
                    productId,
                    imagesCount = addedImages.Count,
                    images = addedImages.Select(i => new
                    {
                        i.Id,
                        i.ImageURL
                    })
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding multiple images");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ✅ GET: api/Products/5/images - Get all images for a product
        [HttpGet("{productId}/images")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductImages(int productId)//هجيب الصور بتاعت المنتج لما يضغط عليها عشان يشوفها طبعا ال id بتاع المنتج هيبقى مع الريكوست اللي بيكون محفوظ في الفرنت اند
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var images = await _unitOfWork.ProductImages.GetImagesByProductIdAsync(productId);

            return Ok(new
            {
                productId,
                imagesCount = images.Count(),
                images = images.Select(i => new
                {
                    i.Id,
                    i.ImageURL,
                    i.ProductId
                })
            });
        }

        // ✅ DELETE: api/Products/images/5 - Delete specific image
        [HttpDelete("{productId}/images/{imageId}")]
        /*
                 images = images.Select(i => new
                {
                    i.Id, // احنا مرجعين هنا ال id بتاع كل صوره فبنفس الفكر لو عايز يمسح صوره يبعتهالنا في اليو ار ال
         */
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> DeleteProductImage(int productId,int imageId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var image = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
            if (image == null)
                return NotFound(new { message = "Image not found" });

            if (image.ProductId != productId)
                return BadRequest(new { message = "Image does not belong to this product" });


            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if(sellerId != product.SellerId)
                return Forbid();


            await _unitOfWork.ProductImages.DeleteAsync(image);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Image deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting image");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ✅ DELETE: api/Products/5/images - Delete all images for a product
        [HttpDelete("{productId}/images")]
        [CheckPermission(Roles.Seller)]
        public async Task<IActionResult> DeleteAllProductImages(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if(sellerId != product.SellerId)
                return Forbid();

            var images = await _unitOfWork.ProductImages.GetImagesByProductIdAsync(productId);

            if (!images.Any())
                return NoContent();

            foreach (var image in images)
            {
                await _unitOfWork.ProductImages.DeleteAsync(image);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new
                {
                    message = "All images deleted successfully",
                    deletedCount = images.Count()
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting images");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

            // ========================================
            // Stock Operations
            // ========================================

            // ✅ POST: api/Products/5/add-stock
            [HttpPost("{productId}/add-stock")]
            [CheckPermission(Roles.Seller)]
            public async Task<IActionResult> AddStock(int productId, [FromBody] StockQuantityDto request)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {productId} not found" });

                int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (product.SellerId != sellerId)
                    return Forbid();

                var result = product.AddStock(request.Quantity);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                try
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Stock added successfully",
                        productId = product.Id,
                        newQuantity = product.QuantityInStock
                    });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error while adding stock");
                    return StatusCode(500, new { message = "Database error occurred" });
                }
            }

            // ✅ POST: api/Products/5/remove-stock
            [HttpPost("{productId}/remove-stock")]
            [CheckPermission(Roles.Seller)]
            public async Task<IActionResult> RemoveStock(int productId, [FromBody] StockQuantityDto request)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {productId} not found" });

                var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if(product.SellerId != sellerId)
                    return Forbid();

                var result = product.RemoveStock(request.Quantity);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                try
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Stock removed successfully",
                        productId = product.Id,
                        newQuantity = product.QuantityInStock
                    });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error while removing stock");
                    return StatusCode(500, new { message = "Database error occurred" });
                }
            }

        // ========================================
        // Reviews Operations
        // ========================================

        // ✅ POST: api/Products/Add-Review
        [HttpPost("{productId}/Add-Review")]
        [CheckPermission(Roles.User)] // اي يوزر مسجل يقدر يضيف ريفيو يعني لازم يكون عنده اكونت وعامل لوجين
        public async Task<IActionResult> AddReview(int productId,[FromBody] RegisterReview review)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _unitOfWork.Products.ViewByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = $"Product with ID {productId} not found" });



            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var existingReview = await _unitOfWork.Reviews
                   .GetByUserAndProductAsync(userId, productId);

            if (existingReview != null)
                return BadRequest(new { message = "You already reviewed this product" });


            var reviewResult = ProductReview.Create(
                productId,
                userId,
                review.Rating,
                review.Comment
            );

            if (!reviewResult.IsSuccess)
                return BadRequest(new { message = reviewResult.Error });

            await _unitOfWork.Reviews.AddAsync(reviewResult.Value!);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Review added successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding review");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ✅ GET: api/Products/5/reviews
        [HttpGet("{productId}/reviews")]
        [AllowAnonymous]//اي حد يقدر يشوف الريفيوهات حتى لو مش مسجل
        public async Task<IActionResult> GetReviews(int productId)
        {
            var product = await _unitOfWork.Products.ViewByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = $"Product with ID {productId} not found" });

            var reviews = await _unitOfWork.Reviews.GetReviewsByProductIdAsync(productId);

            if (!reviews.Any())
                return Ok(new { message = "No reviews found", reviews = new List<object>() });

            var reviewsDto = reviews.Select(r => new
            {
                r.Id,
                r.UserId,
                r.ProductId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.UpdatedAt
            });//طبعا انا باعت بيانات كتير بس الفرنت يتحكم بقى يظهر ايه ومش يظهر ايه

            return Ok(reviewsDto);
        }

        // ✅ PUT: api/Products/Update-Review
        [HttpPut("{reviewId}/Update-Review")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> UpdateReview(int reviewId,[FromBody] UpdateReviewDto updateReviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = await _unitOfWork.Reviews.GetProductReviewByIdAsync(reviewId);
            if (review == null)
                return NotFound(new { message = "Review not found" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (review.UserId != userId)
                return Forbid();

            var result = ProductReview.Update(review, updateReviewDto);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Review updated successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating review");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }

        // ✅ DELETE: api/Products/Delete-Review/5
        [HttpDelete("Delete-Review/{reviewId}")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
                return NotFound(new { message = "Review not found" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (review.UserId != userId)
                return Forbid();

            await _unitOfWork.Reviews.DeleteAsync(review);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Review deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting review");
                return StatusCode(500, new { message = "Database error occurred" });
            }
        }
    }
}
