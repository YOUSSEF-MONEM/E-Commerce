using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryPatternWithUnitOfWork.Core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {

        Task<T?> AddAsync(T entity);
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T?>> GetAllAsync();
        Task<bool> DeleteAsync(T entity);

        Task<T?> UpdateAsync(T entity);

        //  FindAsync مع Include للـ Navigation Properties
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        // أو النسخة البسيطة (بدون includes)
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);



    }
}
