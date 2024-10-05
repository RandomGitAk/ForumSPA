using System.Linq.Expressions;

namespace WebApp.DataAccess.Entities.pages;
public class PagedList<T> : List<T>
{
    public PagedList(IQueryable<T> query, QueryOptions? options = null)
    {
        if (options != null)
        {
            this.CurrentPage = options.CurrentPage;
            this.PageSize = options.PageSize;
        }

        this.Options = options!;
        if (options != null)
        {
            if (!string.IsNullOrEmpty(options.SearchPropertyName) && !string.IsNullOrEmpty(options.SearchTerm))
            {
                this.SearchTerm = options.SearchTerm;
                query = Search(query, options.SearchPropertyName, options.SearchTerm);
            }

            if (options.CategoryId.HasValue)
            {
                query = FilterByProperties(query, nameof(options.CategoryId), options.CategoryId);
            }
        }

        int queryCount = query.Count();
        this.TotalItems = queryCount;
        this.TotalPages = queryCount / this.PageSize;
        if (queryCount % this.PageSize > 0)
        {
            this.TotalPages += 1;
        }

        this.AddRange(query.Skip((this.CurrentPage - 1) * this.PageSize).Take(this.PageSize));
    }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public QueryOptions Options { get; set; }

    public int TotalItems { get; set; }

    public string? SearchTerm { get; set; }

    private static IQueryable<T> Search(IQueryable<T> query, string propertyName, string searchTerm)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var source = propertyName.Split('.').Aggregate((Expression)parameter, Expression.Property);
        var body = Expression.Call(source, "Contains", Type.EmptyTypes, Expression.Constant(searchTerm, typeof(string)));
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
        return query.Where(lambda);
    }

    private static IQueryable<TItem> FilterByProperties<TItem, TValue>(IQueryable<TItem> query, string propertyName, TValue value)
    {
        var parameter = Expression.Parameter(typeof(TItem), "x");

        var property = Expression.Property(parameter, propertyName);
        var getedValue = Expression.Constant(value);
        var convertedValue = Expression.Convert(getedValue, property.Type);
        var filter = Expression.Equal(property, convertedValue);

        var lambda = Expression.Lambda<Func<TItem, bool>>(filter, parameter);
        return query.Where(lambda);
    }
}
