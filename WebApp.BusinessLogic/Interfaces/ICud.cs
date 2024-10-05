namespace WebApp.BusinessLogic.Interfaces;
public interface ICud<TModel, TResult>
        where TModel : class
        where TResult : class
{
    Task<TResult> AddAsync(TModel model);

    Task UpdateAsync(TModel model);

    Task DeleteAsync(int modelId);
}
