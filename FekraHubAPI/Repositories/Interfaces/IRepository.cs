using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels;
using System.Linq.Expressions;
using System.Security.Claims;



namespace FekraHubAPI.Repositories.Interfaces
{
    public enum QueryReturnType
    {
        Single,
        SingleOrDefault,
        First,
        FirstOrDefault,
    }
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
    
        Task<T> GetById(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
        Task DeleteRange(Expression<Func<T, bool>> singlePredicate);
        Task<IQueryable<TResult>> GetRelationAsQueryable<TResult>(
                        Expression<Func<T, bool>>? where = null,
                        List<Expression<Func<T, bool>>>? manyWhere = null,
                        Expression<Func<T, TResult>>? selector = null,
                        Func<IQueryable<T>, IQueryable<T>>? include = null,
                        Expression<Func<T, object>>? orderBy = null,
                        bool asNoTracking = false);
        Task<List<TResult>> GetRelationList<TResult>(
                Expression<Func<T, bool>>? where = null,
                List<Expression<Func<T, bool>>>? manyWhere = null,
                Expression<Func<T, TResult>>? selector = null,
                Func<IQueryable<T>, IQueryable<T>>? include = null,
                Expression<Func<T, object>>? orderBy = null,
                bool asNoTracking = false);
        Task<TResult?> GetRelationSingle<TResult>(
                Expression<Func<T, bool>>? where = null,
                List<Expression<Func<T, bool>>>? manyWhere = null,
                Expression<Func<T, TResult>>? selector = null,
                Func<IQueryable<T>, IQueryable<T>>? include = null,
                QueryReturnType? returnType = QueryReturnType.FirstOrDefault, bool asNoTracking = false);
        Task ManyAdd(List<T> entity);
        Task ManyUpdate(IEnumerable<T> entity);
        Task<bool> IDExists(int id);
        Task<bool> IsTeacherIDExists(string userId);
        Task<bool> IsParentIDExists(string userId);
        Task<bool> IsSecretariatIDExists(string userId);
        Task<bool> IsSecretariat(ApplicationUser user);
        Task<bool> IsTeacher(ApplicationUser user);
        Task<T> GetUser(string id);
        string GetUserIDFromToken(ClaimsPrincipal User);
        Task<PagedResponse<TResult>> GetPagedDataAsync<TResult>(IQueryable<TResult> source, PaginationParameters paginationParameters);
        Task<bool> DataExist(Expression<Func<T, bool>>? singlePredicate = null,List<Expression<Func<T, bool>>>? predicates = null);
        

    }
}
