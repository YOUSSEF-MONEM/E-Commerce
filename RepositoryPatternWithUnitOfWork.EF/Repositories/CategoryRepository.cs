using Categories.DTOs;
using Categories.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Category?> ViewByIdAsync(int id)
        {
            return await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Category?>> GetAllAsync()
        {
            
            return await _dbContext.Categories.Include(c => c.SubCategories).AsNoTracking().ToListAsync();
        }
    }
}
