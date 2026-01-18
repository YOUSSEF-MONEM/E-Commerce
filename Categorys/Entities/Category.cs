using Categories.DTOs;
using Result_Pattern;
using System;
using System.Collections.Generic;

namespace Categories.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public string CategoryName { get; private set; } = string.Empty;
        public string CategoryDescription { get; private set; } = string.Empty;
        public int? ParentCategoryId { get; private set; } //  nullable للـ root categories

        public int SellerId { get; set; }  // FK للـ Saller          //SellerId

        //  Navigation Properties

        public Category? ParentCategory { get; private set; }
        public ICollection<Category> SubCategories { get; private set; } = new List<Category>();

        //  Constructor خاص للـ EF Core
        private Category() { }

        //  Factory Method للـ Root Category (بدون parent)
        public static Result<Category> Create(
            string categoryName,
            string categoryDescription)
        {
            var category = new Category();

            var nameResult = category.SetCategoryName(categoryName);
            if (!nameResult.IsSuccess)
                return Result<Category>.Failure(nameResult.Error);

            var descriptionResult = category.SetCategoryDescription(categoryDescription);
            if (!descriptionResult.IsSuccess)
                return Result<Category>.Failure(descriptionResult.Error);


            // ParentCategoryId = null (root category)
            return Result<Category>.Success(category);
        }

        //  Factory Method للـ Sub Category (مع parent)
        public static Result<Category> Create(
            string categoryName,
            int sellerId,
            string categoryDescription,
            int? parentCategoryId)
        {
            var category = new Category();

            var nameResult = category.SetCategoryName(categoryName);
            if (!nameResult.IsSuccess)
                return Result<Category>.Failure(nameResult.Error);

            var sallerIdSetResult = category.SetSallerId(sellerId);
            if (!sallerIdSetResult.IsSuccess)
                return Result<Category>.Failure(sallerIdSetResult.Error);

            var descriptionResult = category.SetCategoryDescription(categoryDescription);
            if (!descriptionResult.IsSuccess)
                return Result<Category>.Failure(descriptionResult.Error);

            var parentResult = category.SetCategoryParentId(parentCategoryId);
            if (!parentResult.IsSuccess)
                return Result<Category>.Failure(parentResult.Error);

            return Result<Category>.Success(category);
        }

        public Result<Category> Update(Category category , UpdateCategoryDto updateCategoryDto)
        {
            if (string.IsNullOrWhiteSpace(updateCategoryDto.CategoryName))
            {
                var result = category.SetCategoryName(updateCategoryDto.CategoryName);
                if (!result.IsSuccess)
                    return Result<Category>.Failure(result.Error);

            }

            if (string.IsNullOrWhiteSpace(updateCategoryDto.CategoryDescription))
            {
                var result = category.SetCategoryDescription(updateCategoryDto.CategoryDescription);
                if (!result.IsSuccess)
                    return Result<Category>.Failure(result.Error);
            }

            if(updateCategoryDto.ParentCategoryId != category.ParentCategoryId)
            {
                var parentResult = category.SetCategoryParentId(updateCategoryDto.ParentCategoryId);
                if (!parentResult.IsSuccess)
                    return Result<Category>.Failure(parentResult.Error);
            }

            return Result<Category>.Success(category);
        }
        

        // SetCategoryName
        public Result SetCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return Result.Failure("Category Name is required");

            if (categoryName.Length < 3)
                return Result.Failure("Category Name must be at least 3 characters");

            if (categoryName.Length > 50)
                return Result.Failure("Category Name must not exceed 50 characters");

            CategoryName = categoryName.Trim();
            return Result.Success();
        }
        public Result SetSallerId(int sellerId)
        { 
            if (sellerId <= 0)
               return Result.Failure("Invalid seller id");

            SellerId = sellerId;
            return Result.Success();
        }

        //  SetCategoryDescription
        public Result SetCategoryDescription(string categoryDescription)
        {
            if (string.IsNullOrWhiteSpace(categoryDescription))
                return Result.Failure("Category Description is required");

            if (categoryDescription.Length < 10)
                return Result.Failure("Category Description must be at least 10 characters");

            if (categoryDescription.Length > 500)
                return Result.Failure("Category Description must not exceed 500 characters");

            CategoryDescription = categoryDescription.Trim();
            return Result.Success();
        }

        //  SetCategoryParentId - الحل الكامل!
        public Result SetCategoryParentId(int? parentId)
        {
            //  لو null = root category
            if (parentId == null) // == null يعني root category يعني ملوش parent يعني ال ParentCategoryId مش مبعوت
            {
                ParentCategoryId = null;
                return Result.Success();
            }

            //  Validation
            if (parentId <= 0)
                return Result.Failure("Invalid Parent Category ID");

            //  منع الـ self-reference (Category تبقى parent لنفسها!)
            if (Id != 0 && parentId == Id)
                return Result.Failure("Category cannot be its own parent");

            ParentCategoryId = parentId;
            return Result.Success();
        }

        //  Helper Methods

        /// <summary>
        /// Check if this is a root category (no parent)
        /// </summary>
        public bool IsRootCategory() => ParentCategoryId == null;

        /// <summary>
        /// Check if this category has sub-categories
        /// </summary>
        public bool HasSubCategories() => SubCategories.Count > 0;

        /// <summary>
        /// Get the depth level of this category
        /// </summary>
        public int GetDepthLevel()
        {
            if (IsRootCategory())
                return 0;

            return ParentCategory?.GetDepthLevel() + 1 ?? 0;
        }
    }
}













