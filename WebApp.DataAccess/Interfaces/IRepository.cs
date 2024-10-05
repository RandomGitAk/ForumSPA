using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.Interfaces;
public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<IEnumerable<TEntity>> GetAllAsync();

    Task<TEntity?> GetByIdAsync(int id);

    Task AddAsync(TEntity entity);

    void Delete(TEntity entity);

    Task DeleteByIdAsync(int id);

    Task UpdateAsync(TEntity entity);
}
