using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class CategoryRepository : BaseRepository<Category>, ICategory
{
    private readonly ApplicationContext context;

    public CategoryRepository(ApplicationContext context)
        : base(context)
    {
        this.context = context;
    }

    public PagedList<Category> GetCategoriesPaged(QueryOptions options)
    {
        return new PagedList<Category>(
            this.context.Categories.AsNoTracking()
         .OrderByDescending(e => e.Id), options);
    }
}
