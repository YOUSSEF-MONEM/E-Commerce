using Carts.Entities;
using Categories.DTOs;
using Categories.Entities;
using Products.DTOs;
using Products.Entities;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> ViewByIdAsync(int id);

    }
}
