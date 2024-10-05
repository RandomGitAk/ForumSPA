using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.CategoryModels;

namespace WebApp.BusinessLogic.Interfaces;
public interface ICategoryService : ICrud<CategoryModel>
{
    CategoryModelPagination GetCategoriesPaged(PaginationParamsFromQueryModel paginationParams);
}
