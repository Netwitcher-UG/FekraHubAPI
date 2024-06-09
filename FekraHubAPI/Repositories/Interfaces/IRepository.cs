using FekraHubAPI.Data.Models;
using System.Linq.Expressions;

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
        Task ManyAdd(T entity);
        void ManyUpdate(T entity);
        Task SaveManyAdd();
        Task<bool> IDExists(int id);
        Task<bool> IsTeacherIDExists(string userId);
        Task<bool> IsSecretariatIDExists(string userId);
        
        Task<T> GetUser(string id);
    }
}
