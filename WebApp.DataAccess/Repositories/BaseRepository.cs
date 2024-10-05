using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class BaseRepository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
{
    private readonly ApplicationContext context;

    public BaseRepository(ApplicationContext context)
    {
        this.context = context;
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        _ = await this.context.Set<TEntity>().AddAsync(entity);
        _ = await this.context.SaveChangesAsync();
    }

    public virtual void Delete(TEntity entity)
    {
        _ = this.context.Set<TEntity>().Remove(entity);
        _ = this.context.SaveChanges();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var entity = await this.GetByIdAsync(id);
        if (entity != null)
        {
            _ = this.context.Set<TEntity>().Remove(entity);
            _ = await this.context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await this.context.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await this.context.Set<TEntity>().FindAsync(id);
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        var existingEntity = await this.context.Set<TEntity>().FindAsync(entity?.Id);
        if (existingEntity != null)
        {
            this.context.Entry(existingEntity).CurrentValues.SetValues(entity!);
            _ = await this.context.SaveChangesAsync();
        }
    }
}
