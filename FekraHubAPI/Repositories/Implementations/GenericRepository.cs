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
       
        public async Task<IQueryable<TResult>> GetRelationAsQueryable<TResult>(
                        Expression<Func<T, bool>>? where = null,
                        List<Expression<Func<T, bool>>>? manyWhere = null,
                        Expression<Func<T, TResult>>? selector = null,
                        Func<IQueryable<T>, IQueryable<T>>? include = null,
                        Expression<Func<T, object>>? orderBy = null,
                        bool asNoTracking = false)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
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

            if (orderBy != null)
            {
                query = query.OrderByDescending(orderBy);
            }

            if (selector == null)
            {
                throw new ArgumentException("Selector must be provided.");
            }

            return query.Select(selector);
        }
        public async Task<List<TResult>> GetRelationList<TResult>(
     Expression<Func<T, bool>>? where = null,
     List<Expression<Func<T, bool>>>? manyWhere = null,
     Expression<Func<T, TResult>>? selector = null,
     Func<IQueryable<T>, IQueryable<T>>? include = null,
     Expression<Func<T, object>>? orderBy = null,
     bool asNoTracking = false)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTrackingWithIdentityResolution();
            }

            if (where != null)
            {
                query = query.Where(where);
            }

            if (manyWhere != null && manyWhere.Any())
            {
                var combinedWhere = CombinePredicates(manyWhere);
                query = query.Where(combinedWhere);
            }

            if (include != null)
            {
                query = include(query);
            }

            if (orderBy != null)
            {
                query = query.OrderByDescending(orderBy);
            }

            if (selector == null)
            {
                throw new ArgumentException("Selector must be provided.");
            }

            return await query.Select(selector).ToListAsync();
        }

        public async Task<TResult?> GetRelationSingle<TResult>(
            Expression<Func<T, bool>>? where = null,
            List<Expression<Func<T, bool>>>? manyWhere = null,
            Expression<Func<T, TResult>>? selector = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            QueryReturnType? returnType = QueryReturnType.FirstOrDefault,
            bool asNoTracking = false)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTrackingWithIdentityResolution();
            }

            if (where != null)
            {
                query = query.Where(where);
            }

            if (manyWhere != null && manyWhere.Any())
            {
                var combinedWhere = CombinePredicates(manyWhere);
                query = query.Where(combinedWhere);
            }

            if (include != null)
            {
                query = include(query);
            }

            if (selector == null)
            {
                throw new ArgumentException("Selector must be provided.");
            }

            try
            {
                switch (returnType)
                {
                    case QueryReturnType.SingleOrDefault:
                        return await query.Select(selector).SingleOrDefaultAsync();
                    case QueryReturnType.Single:
                        return await query.Select(selector).SingleAsync();
                    case QueryReturnType.FirstOrDefault:
                        return await query.Select(selector).FirstOrDefaultAsync();
                    case QueryReturnType.First:
                        return await query.Select(selector).FirstAsync();
                    default:
                        throw new NotSupportedException($"Return type '{returnType}' is not supported.");
                }
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException("The result type does not match the expected type. Check your selector or return type.", ex);
            }
        }

        private Expression<Func<T, bool>> CombinePredicates(List<Expression<Func<T, bool>>> predicates)
        {
            if (predicates == null || !predicates.Any())
            {
                return x => true; 
            }

            var firstPredicate = predicates.First();
            var body = firstPredicate.Body;
            var param = firstPredicate.Parameters[0];

            foreach (var predicate in predicates.Skip(1))
            {
                var visitor = new ReplaceParameterVisitor(predicate.Parameters[0], param);
                body = Expression.AndAlso(body, visitor.Visit(predicate.Body));
            }

            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
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
            var IsTeacher = await _context.UserRoles.AnyAsync(x=> x.UserId == userId && x.RoleId == "4");
            
            return IsTeacher;
        }
        public async Task<bool> IsParentIDExists(string userId)
        {
            var IsTeacher = await _context.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == "3");

            return IsTeacher;
        }
        public async Task<bool> IsSecretariatIDExists(string userId)
        {
            var isSecretariat = await _context.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == "2");
            return isSecretariat;
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

        public async Task<PagedResponse<TResult>> GetPagedDataAsync<TResult>(IQueryable<TResult> source, PaginationParameters paginationParameters)
        {
            var count =  await source.CountAsync();
            var items =  await source.Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                                    .Take(paginationParameters.PageSize)
                                    .ToListAsync();

            return new PagedResponse<TResult>(items, count, paginationParameters.PageNumber, paginationParameters.PageSize);
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
                var combinedPredicate = predicates
                    .Aggregate((current, next) => Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(current.Body, Expression.Invoke(next, current.Parameters)),
                        current.Parameters));

                return await _dbSet.AnyAsync(combinedPredicate);
            }
            return await _dbSet.AnyAsync();
        }

       
    }
}
