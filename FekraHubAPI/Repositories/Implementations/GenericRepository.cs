using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FekraHubAPI.Repositories.Implementations
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly UserManager<ApplicationUser> _userManager;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
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
        public async Task<IQueryable<T>> GetRelation()
        {
            return await Task.FromResult(_dbSet.AsQueryable());
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
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
