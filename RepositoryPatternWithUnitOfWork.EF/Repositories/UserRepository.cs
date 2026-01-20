using Microsoft.EntityFrameworkCore;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System.Threading.Tasks;
using Users.DTOs;
using Users.Entities;
using Result_Pattern;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

         //  Override GetByIdAsync عشان Include Phones & Roles
        public override async Task<User?> GetByIdAsync(int id)
        {
            //الـ FirstOrDefaultAsync بيرجع null تلقائياً لو مالقاش حاجة
            var user = await _dbContext.Users
                .Include(u => u.Roles) //  Include Roles
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == id); ;    
            return user;
        }

        public  async Task<User?> Login(string email , string password)
        {
            //الـ FirstOrDefaultAsync بيرجع null تلقائياً لو مالقاش حاجة
            var user = await _dbContext.Users
                .Include(u => u.Roles) //  Include Roles
                .Include(u => u.RefreshTokens) // ✅ Include للـ Tokens
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Email == email );

            if (user == null)
                return null;


            if (!user.VerifyPassword(password))
                return null;
            /*
             * 
             *  u.Password == password غبي
             Password في الداتا بيز مش الباسورد الحقيقي

هو HASH ناتج من:

BCrypt.Net.BCrypt.HashPassword(password);
             */
            return user;
        }
        public  async Task<bool?> FinedByEmail(string email )
        {
            var fined = await _dbContext.Users
                 .AnyAsync(u => u.Email == email); 
            return fined;
        }

        //  Override GetAllAsync
        public override async Task<IEnumerable<User?>> GetAllAsync()
        {
            var users = await _dbContext.Users.AsNoTracking()
                .Include(u => u.Roles) //  Include Roles
                .AsSplitQuery()
                .ToListAsync();
            //الـ ToListAsync() مش هيرجع null أبداً
            //هيرجع empty list لو مافيش

            return users;
        }

        public async Task<User?> ViewByIdAsync(int id)
        {
            return await _dbContext.Users.AsNoTracking()
                .Include(u => u.Roles) //  Include Roles
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool?> FinedByPhone(string phone)
        {
            var fined = await _dbContext.Users
                   .AnyAsync(u => u.PhoneNumber == phone); 
            return fined;
        }

        public async Task<User?> GetByIdWithRefreshTokensAsync(int id)
        {
            return await _dbContext.Users
                .Include(u => u.RefreshTokens) // ✅ Include للـ Tokens
                .Include(u => u.Roles)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}

