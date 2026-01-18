using Result_Pattern;
using System;
using Products.Entities;
using Products.DTOs;

namespace Products.Entities
{
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // ✅ Optional: Rating or Comment (لكن واحد منهم على الأقل)
        public double? Rating { get; private set; } // ⬅️ Nullable (0-10)
        public string? Comment { get; private set; } // ⬅️ Nullable

        // ✅ Navigation Properties
        public Product Product { get; private set; } = null!;

        // ✅ Constructor خاص للـ EF Core
        private ProductReview()
        {
            CreatedAt = DateTime.UtcNow;
        }

        // ✅ Factory Method
        public static Result<ProductReview> Create(
            int productId,
            int userId,
            double? rating = null,
            string? comment = null)
        {
            var review = new ProductReview();

            // Validate IDs
            if (productId <= 0)
                return Result<ProductReview>.Failure("Invalid Product ID");

            if (userId <= 0)
                return Result<ProductReview>.Failure("Invalid User ID");

            // ✅ واحد على الأقل لازم يكون موجود (Rating أو Comment)
            if (!rating.HasValue && string.IsNullOrWhiteSpace(comment))
                return Result<ProductReview>.Failure("Review must have either rating or comment");

            // Set IDs
            review.ProductId = productId;
            review.UserId = userId;

            // ✅ Set Rating (if provided)
            if (rating.HasValue)
            {
                var ratingResult = review.SetRating(rating.Value);
                if (!ratingResult.IsSuccess)
                    return Result<ProductReview>.Failure(ratingResult.Error);
            }

            // ✅ Set Comment (if provided)
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var commentResult = review.SetComment(comment);
                if (!commentResult.IsSuccess)
                    return Result<ProductReview>.Failure(commentResult.Error);
            }

            return Result<ProductReview>.Success(review);
        }

        public static Result<ProductReview> Update(ProductReview productReview , UpdateReviewDto updateReviewDto) 
        {
            if (!string.IsNullOrWhiteSpace(updateReviewDto.Comment))
            {
                var result = productReview.SetComment(updateReviewDto.Comment);
                if (!result.IsSuccess)
                    return Result<ProductReview>.Failure(result.Error);
            }

            if(updateReviewDto.Rating.HasValue)
            {
                var result = productReview.SetRating(updateReviewDto.Rating.Value);
                if (!result.IsSuccess)
                    return Result<ProductReview>.Failure(result.Error);
            }

            return Result<ProductReview>.Success(productReview);
        }

        // ✅ Set Rating (من 0 إلى 10)
        public Result SetRating(double rating)
        {
            if (rating < 0 || rating > 10)
                return Result.Failure("Rating must be between 0 and 10");

            Rating = rating;
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Remove Rating (make it optional)
        public Result RemoveRating()
        {
            // ✅ Check: لازم يكون فيه Comment لو هنحذف الـ Rating
            if (string.IsNullOrWhiteSpace(Comment))
                return Result.Failure("Cannot remove rating. Review must have either rating or comment");

            Rating = null;
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Set Comment
        public Result SetComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return Result.Failure("Comment cannot be empty");

            if (comment.Length < 3)
                return Result.Failure("Comment must be at least 3 characters");

            if (comment.Length > 1000)
                return Result.Failure("Comment must not exceed 1000 characters");

            Comment = comment.Trim();
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Remove Comment (make it optional)
        public Result RemoveComment()
        {
            // ✅ Check: لازم يكون فيه Rating لو هنحذف الـ Comment
            if (!Rating.HasValue)
                return Result.Failure("Cannot remove comment. Review must have either rating or comment");

            Comment = null;
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Update Review
        public Result Update(double? rating = null, string? comment = null)
        {
            // ✅ Check: واحد على الأقل لازم يكون موجود
            if (!rating.HasValue && string.IsNullOrWhiteSpace(comment))
                return Result.Failure("Review must have either rating or comment");

            // Update Rating
            if (rating.HasValue)
            {
                var ratingResult = SetRating(rating.Value);
                if (!ratingResult.IsSuccess)
                    return ratingResult;
            }
            else
            {
                // لو مش باعت rating، احذفه (لو موجود comment)
                if (!string.IsNullOrWhiteSpace(comment))
                    Rating = null;
            }

            // Update Comment
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var commentResult = SetComment(comment);
                if (!commentResult.IsSuccess)
                    return commentResult;
            }
            else
            {
                // لو مش باعت comment، احذفه (لو موجود rating)
                if (rating.HasValue)
                    Comment = null;
            }

            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Helper Methods
        public bool HasRating() => Rating.HasValue;

        public bool HasComment() => !string.IsNullOrWhiteSpace(Comment);

        public bool HasBoth() => HasRating() && HasComment();

        private void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}