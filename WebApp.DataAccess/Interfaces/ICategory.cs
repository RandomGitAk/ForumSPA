using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;

namespace WebApp.DataAccess.Interfaces;
public interface ICategory : IRepository<Category>
{
    PagedList<Category> GetCategoriesPaged(QueryOptions options);
}
