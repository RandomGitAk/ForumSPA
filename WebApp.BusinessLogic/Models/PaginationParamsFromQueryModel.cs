using System.ComponentModel;

namespace WebApp.BusinessLogic.Models;
public class PaginationParamsFromQueryModel
{
    [DefaultValue(0)]
    public int CategoryId { get; set; }

    [DefaultValue(0)]
    public int Page { get; set; } = 0;

    [DefaultValue(5)]
    public int PerPage { get; set; } = 5;

    public string? SearchPropertyName { get; set; } = null!;

    public string? SearchTerm { get; set; } = null!;
}
