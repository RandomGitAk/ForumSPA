namespace WebApp.DataAccess.Entities.pages;
public class QueryOptions
{
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 5;

    public string SearchPropertyName { get; set; } = null!;

    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }
}
