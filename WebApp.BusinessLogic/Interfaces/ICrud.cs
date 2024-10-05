namespace WebApp.BusinessLogic.Interfaces;
public interface ICrud<TModel>
        where TModel : class
{
    Task<TModel> AddAsync(TModel model);

    Task<TModel> GetByIdAsync(int id);

    Task<IEnumerable<TModel>> GetAllAsync();

    Task UpdateAsync(TModel model);

    Task DeleteAsync(int modelId);
}
