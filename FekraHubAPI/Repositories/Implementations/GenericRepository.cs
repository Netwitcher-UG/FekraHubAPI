using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using FekraHubAPI.MapModels;
using System;



namespace FekraHubAPI.Repositories.Implementations
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly UserManager<ApplicationUser> _userManager;

        public GenericRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _userManager = userManager;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

     

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteRange(Expression<Func<T, bool>> singlePredicate)
        {
            var entity = await _dbSet.Where(singlePredicate).ToListAsync();
            if (entity != null)
            {
                _dbSet.RemoveRange(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IQueryable<TResult>> GetRelation<TResult>(
                Expression<Func<T, bool>>? where = null,
                List<Expression<Func<T, bool>>>? manyWhere = null,
                Expression<Func<T, TResult>>? selector = null,
                Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (where != null)
            {
                query = query.Where(where);
            }

            if (manyWhere != null)
            {
                foreach (var predicate in manyWhere)
                {
                    if (predicate != null)
                    {
                        query = query.Where(predicate);
                    }
                }
            }
            if (include != null)
            {
                query = include(query);
            }
            if (selector != null)
            {
                return await Task.FromResult(query.Select(selector));
            }

            return await Task.FromResult((IQueryable<TResult>)query);
        }
        public async Task ManyAdd(List<T> entity)
        {
            await _dbSet.AddRangeAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ManyUpdate(IEnumerable<T> entity)
        {
            _dbSet.UpdateRange(entity);
            await _context.SaveChangesAsync();
        }
        
        public async Task<bool> IDExists(int id)
        {
            return await _dbSet.FindAsync(id) != null;
        }
        public async Task<bool> IsTeacherIDExists(string userId)
        {
            var roles = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RolesEnum.Teacher.ToString());
            if (roles == null)
            {
                return false;
            }
            var user = await _context.Users.FindAsync(userId);
            var userRoles = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roles.Id);
            return user != null && userRoles != null;
        }
        public async Task<bool> IsSecretariatIDExists(string userId)
        {

            var roles = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RolesEnum.Secretariat.ToString());
            if (roles == null)
            {
                return false;
            }
            var user = await _context.Users.FindAsync(userId);
            var userRoles = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roles.Id);
            return user != null && userRoles != null;


            //var isSecretariat = await _userManager.IsInRoleAsync(user, DefaultRole.Secretariat);
            //return isSecretariat;
        }
        public async Task<bool> IsSecretariat(ApplicationUser user)
        {
            var isSecretariat = await _userManager.IsInRoleAsync(user, DefaultRole.Secretariat);
            return isSecretariat;
        }
        public async Task<bool> IsTeacher(ApplicationUser user)
        {
            var isSecretariat = await _userManager.IsInRoleAsync(user, DefaultRole.Teacher);
            return isSecretariat;
        }
        public async Task<T> GetUser(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public string GetUserIDFromToken(ClaimsPrincipal User)
        {
            return User.FindFirst("id")?.Value;
        }

        public async Task<PagedResponse<T>> GetPagedDataAsync(IQueryable<T> source, PaginationParameters paginationParameters)
        {
            var count = await source.CountAsync();
            var items =  source.Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                                    .Take(paginationParameters.PageSize);

            return new PagedResponse<T>(items, count, paginationParameters.PageNumber, paginationParameters.PageSize);
        }

        public async Task<bool> DataExist(
            Expression<Func<T, bool>>? singlePredicate = null,
            List<Expression<Func<T, bool>>>? predicates = null)
        {
            if (singlePredicate != null)
            {
                return await _dbSet.AnyAsync(singlePredicate);
            }

            if (predicates != null && predicates.Any())
            {
                foreach (var predicate in predicates)
                {
                    bool exists = await _dbSet.AnyAsync(predicate);
                    if (!exists)
                    {
                        return false; 
                    }
                }
                return true; 
            }
            return await _dbSet.AnyAsync();
        }


    }
}
