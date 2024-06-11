using FekraHubAPI.Data.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace FekraHubAPI.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
        Task<IQueryable<T>> GetRelation();
        Task ManyAdd(List<T> entity);
        Task ManyUpdate(IEnumerable<T> entity);
        Task<bool> IDExists(int id);
        Task<bool> IsTeacherIDExists(string userId);
        Task<bool> IsSecretariatIDExists(ApplicationUser user);
        
        Task<T> GetUser(string id);
        string GetUserIDFromToken(ClaimsPrincipal User);
    }
}
