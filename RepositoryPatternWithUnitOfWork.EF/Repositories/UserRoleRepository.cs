using Microsoft.EntityFrameworkCore;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Repositories
{
    public class UserRoleRepository : BaseRepository<UserRoles>, IUserRoleRepository
    {
        public UserRoleRepository(ECommeeceDbContext dbContext) : base(dbContext)
        {
        }

        //public async Task<List<UserRoles>> ViewByIdAsync(int id)
        //{
        //    return await _dbContext.UserRoles.AsNoTracking().Where(ur => ur.UserId == id).ToListAsync();
        //}
    }
}
