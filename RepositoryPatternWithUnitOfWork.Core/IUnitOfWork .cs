using NPOI.SS.Formula.Functions;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using RepositoryPatternWithUnitOfWork.Core.InterfacesRepository;
using System;
using System.Collections.Generic;
using System.Text;


namespace RepositoryPatternWithUnitOfWork.Core
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        ICartProductRepository CartProducts { get; }
        ICartRepository Carts { get; }
        ICategoryRepository Categories { get; }
        IOrderItemRepository OrderItems { get; }
        IOrderRepository Orders { get; }
        IProductRepository Products { get; }
        IReviewRepository Reviews { get; }
        IUserRepository Users { get; }
        IUserRoleRepository UserRoles { get; }
        IPaymentRepository Payments { get; }
        IProductImageRepository ProductImages { get; }



        // ضيف باقي الـ repositories اللي عندك



        // Save Changes
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}
