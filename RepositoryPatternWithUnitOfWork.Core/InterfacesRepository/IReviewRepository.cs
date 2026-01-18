using Carts.Entities;
using Products.DTOs;
using Products.Entities;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface IReviewRepository : IBaseRepository<ProductReview>
    {
        public Task<List<ProductReview>> GetReviewsByProductIdAsync(int productId);
        //public Result<ProductReview> UpdateReview(UpdateReviewDto reviewDto);
        public Task<ProductReview?> GetProductReviewByIdAsync(int reviewId);

        Task<ProductReview?> GetByUserAndProductAsync(int userId, int productId);

    }
}
