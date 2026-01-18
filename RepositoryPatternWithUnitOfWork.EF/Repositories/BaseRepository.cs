using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using RepositoryPatternWithUnitOfWork.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    /*
     ⚡ يستخدم DbContext بس مش مسؤول عن SaveChanges

ليه مفيش SaveChanges في الـ Repository؟

عشان نقدر نعمل أكتر من عملية في Transaction واحدة
الـ SaveChanges هيكون مسؤولية الـ UnitOfWork
     */
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ECommeeceDbContext _dbContext;

        public BaseRepository(ECommeeceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public virtual async Task<T?> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(T entity)
        {

            _dbContext.Set<T>().Remove(entity);

            return true;
        }

        public virtual async Task<IEnumerable<T?>> GetAllAsync()
        {

            // AsNoTracking عشان احسن الاداء في حالة اني جايب الداتا للقراءة مش للتعديل
            return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            //مش محتاج AsNoTracking عشان FindAsync بيجيب الانتيتي متتبعة عشان انا بستخدمها في التعديل
            return await _dbContext.Set<T>().FindAsync(id);

        }

        //




        //يستخدم في حالة ان انا جايب الانتيتي من غير تتبع
        // يعني جايبها كـ NoTracking
        // فمحتاج اعملها Attach عشان اعملها Update
        // لو الانتيتي متتبعة مش محتاج اعمل Attach
        //يعني مش مستعمل ال DbContext في جلب البيانات
        /*
         طالما جبت الحاجة من الداتابيز بنفس الـ DbContext

❌ متقولش له Update
هو عارف لوحده

        .Update():

بيعمل Modified لكل الأعمدة

حتى اللي متغيرتش

ممكن يعمل SQL أكبر

ومش optimal
        راجع ال ChangeTracker
         */
        public virtual async Task<T?> UpdateAsync(T entity)
        {
            return  await Task.Run(() =>
            {
                _dbContext.Set<T>().Update(entity);
                return entity;
            });
        }

        // ✅ FindAsync مع دعم Includes
        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            // إضافة الـ Includes
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // تطبيق الـ Predicate والحصول على النتائج
            return await query.Where(predicate).ToListAsync();
        }

        // ✅ النسخة البسيطة (بدون includes)
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>()
                .Where(predicate)
                .ToListAsync();
        }



    }
}


