using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using Products.DTOs;
using Products.Entities;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class ReviewRepository : BaseRepository<ProductReview>, IReviewRepository
    {
        public ReviewRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<ProductReview>> GetReviewsByProductIdAsync(int productId)
        {
            return await _dbContext.Reviews
                .Where(r => r.ProductId == productId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductReview?> GetProductReviewByIdAsync(int reviewId)
        {
            return await _dbContext.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<ProductReview?> GetByUserAndProductAsync(int userId, int productId)
        {
            return await _dbContext.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
        }

    }

}
