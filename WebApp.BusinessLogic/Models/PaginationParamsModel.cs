namespace WebApp.BusinessLogic.Models;
public class PaginationParamsModel
{
    public int Total { get; set; }

    public int Page { get; set; }

    public int PerPage { get; set; }

    public int TotalPages { get; set; }

    public string SearchTerm { get; set; } = null!;

    public int? CategoryId { get; set; }
}
