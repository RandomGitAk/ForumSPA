namespace WebApp.BusinessLogic.Interfaces;
public interface IRead<TModel>
        where TModel : class
{
    Task<TModel> GetByIdAsync(int id);

    Task<IEnumerable<TModel>> GetAllAsync();
}
