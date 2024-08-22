using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels;
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
        Task<IQueryable<TResult>> GetRelation<TResult>(Expression<Func<T, bool>>? singlePredicate = null, List<Expression<Func<T, bool>>>? predicate = null , Expression<Func<T, TResult>>? selector = null);
        Task ManyAdd(List<T> entity);
        Task ManyUpdate(IEnumerable<T> entity);
        Task<bool> IDExists(int id);
        Task<bool> IsTeacherIDExists(string userId);
        Task<bool> IsSecretariatIDExists(string userId);
        Task<bool> IsSecretariat(ApplicationUser user);
        Task<bool> IsTeacher(ApplicationUser user);
        Task<T> GetUser(string id);
        string GetUserIDFromToken(ClaimsPrincipal User);
        Task<PagedResponse<T>> GetPagedDataAsync(IQueryable<T> source, PaginationParameters paginationParameters);


    }
}
