namespace WebApp.BusinessLogic.Models.CategoryModels;
public class CategoryModelPagination : PaginationParamsModel
{
    public IEnumerable<CategoryModel>? Items { get; set; }
}
