using AutoMapper;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.CategoryModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class CategoryService : ICategoryService
{
    private readonly ICategory categoryRepository;
    private readonly IMapper mapper;

    public CategoryService(ICategory categoryRepository, IMapper mapper)
    {
        this.categoryRepository = categoryRepository;
        this.mapper = mapper;
    }

    public async Task<CategoryModel> AddAsync(CategoryModel model)
    {
        var category = this.mapper.Map<Category>(model);
        await this.categoryRepository.AddAsync(category);
        return this.mapper.Map<CategoryModel>(category);
    }

    public async Task DeleteAsync(int modelId)
    {
        var existCategory = await this.categoryRepository.GetByIdAsync(modelId);
        if (existCategory == null)
        {
            throw new InvalidOperationException("Category not found.");
        }

        await this.categoryRepository.DeleteByIdAsync(modelId);
    }

    public async Task<IEnumerable<CategoryModel>> GetAllAsync()
    {
        var categories = await this.categoryRepository.GetAllAsync();
        return this.mapper.Map<IEnumerable<CategoryModel>>(categories);
    }

    public async Task<CategoryModel> GetByIdAsync(int id)
    {
        var category = await this.categoryRepository.GetByIdAsync(id);
        return this.mapper.Map<CategoryModel>(category);
    }

    public CategoryModelPagination GetCategoriesPaged(PaginationParamsFromQueryModel paginationParams)
    {
        var pageListCategory = this.categoryRepository.GetCategoriesPaged(new QueryOptions
        {
            CurrentPage = (paginationParams?.Page ?? 0) + 1,
            PageSize = paginationParams?.PerPage ?? 0,
            SearchPropertyName = paginationParams?.SearchPropertyName!,
            SearchTerm = paginationParams?.SearchTerm!,
        });

        return this.mapper.Map<CategoryModelPagination>(pageListCategory);
    }

    public async Task UpdateAsync(CategoryModel model)
    {
        var existCategory = await this.categoryRepository.GetByIdAsync(model?.Id ?? 0);
        if (existCategory == null)
        {
            throw new InvalidOperationException("Category not found.");
        }

        var category = this.mapper.Map<Category>(model);
        await this.categoryRepository.UpdateAsync(category);
    }
}
