using Carts.Entities;
using NPOI.SS.Formula.Functions;
using Result_Pattern;
using System;
using System.Collections.Generic;
using System.Text;
using Users.DTOs;
using Users.Entities;
namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> ViewByIdAsync(int id);
        Task<User?> Login(string email, string password);
        Task<bool?> FinedByEmail(string email);
        Task<bool?> FinedByPhone(string phone);

        Task<User?> GetByIdWithRefreshTokensAsync(int id);

    }
}
