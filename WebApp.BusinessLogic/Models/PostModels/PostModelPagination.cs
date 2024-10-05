namespace WebApp.BusinessLogic.Models.PostModels;
public class PostModelPagination : PaginationParamsModel
{
    public IEnumerable<PostModel> Items { get; set; } =
        [];
}
