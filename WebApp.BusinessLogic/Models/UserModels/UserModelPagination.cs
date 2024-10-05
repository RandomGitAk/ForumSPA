namespace WebApp.BusinessLogic.Models.UserModels;
public class UserModelPagination : PaginationParamsModel
{
    public IEnumerable<UserModel> Items { get; set; } =
        [];
}
