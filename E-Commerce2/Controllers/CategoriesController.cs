using Categories.DTOs;
using Categories.Entities;
using E_Commerce2.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryPatternWithUnitOfWork.Core;
using System.Security.Claims;

namespace E_Commerce2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [CheckPermission(Users.Constants.Roles.Seller)]
        public async Task<IActionResult> CreateCategory([FromBody] RegisterCategoryDto registerCategoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = Category.Create(
                registerCategoryDto.CategoryName,
                sellerId,
                registerCategoryDto.CategoryDescription,
                registerCategoryDto.ParentCategoryId
            );
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }


            await _unitOfWork.Categories.AddAsync(result.Value!);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                message = "Category created successfully",
                CategoryId = result.Value!.Id,
                categoryName = result.Value.CategoryName
            });
        }

        [HttpPut("{categoryId}")]//////////////////////////////////////////////////////////////////////////////////////
        [CheckPermission(Users.Constants.Roles.Seller)]
        public async Task<IActionResult> UpdateCategory(int categoryId,[FromBody] UpdateCategoryDto updateCategoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if(category.SellerId != sellerId)
                return Forbid();

            var result = category.Update(category,updateCategoryDto);//////////////////////////////////////////////////////////
                
            if (!result.IsSuccess)
                 return BadRequest(result.Error); 

           await _unitOfWork.SaveChangesAsync();

           return Ok(new { message = "Category updated successfully"});
        }

        [HttpDelete("{id}")]
        [CheckPermission(Users.Constants.Roles.Seller)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            int sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (category.SellerId != sellerId)
                return Forbid();

            await _unitOfWork.Categories.DeleteAsync(category);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoriesDto = categories.Select(c => new
            {
                c!.Id,
                c.CategoryName,
                c.CategoryDescription,
                c.ParentCategoryId,
                SubCategories = c.SubCategories.Select(sc => new
                {
                    sc.CategoryName
                })
            });
            return Ok(categoriesDto);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _unitOfWork.Categories.ViewByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }
            
            return Ok(new
            {
                category.Id,
                category.CategoryName,
                category.CategoryDescription,
                category.ParentCategoryId,
                SubCategories = category.SubCategories.Select(sc => new
                {
                    sc.CategoryName
                })
            });
        }


    }
}