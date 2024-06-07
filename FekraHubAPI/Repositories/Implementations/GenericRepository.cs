using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FekraHubAPI.Repositories.Implementations
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

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
        public async Task ManyAdd(T entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public async Task SaveManyAdd()
        {
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
        }
        public async Task<T> GetUser(string id)
        {
            return await _dbSet.FindAsync(id);
        }

    }
}
