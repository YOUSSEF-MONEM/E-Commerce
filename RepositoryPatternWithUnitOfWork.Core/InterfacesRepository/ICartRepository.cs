using Carts.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface ICartRepository : IBaseRepository<Cart>
    {
         Task<Cart?> ViewByIdAsync(int id);
        Task<Cart?> GetCartByUserIdAsync(int userId);
    }
}
