using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.CategoryModels;
using WebApp.WebApi.ViewModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// Controller for managing categories.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService categoryService;
    private readonly ILogger<CategoriesController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesController"/> class.
    /// </summary>
    /// <param name="categoryService">The category service.</param>
    /// <param name="logger">The logger.</param>
    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        this.categoryService = categoryService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <returns>A list of categories.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryModel>>> Get()
    {
        try
        {
            var categories = await this.categoryService.GetAllAsync();
            return this.Ok(categories);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An error occurred while getting categories.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Gets paginated categories.
    /// </summary>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <returns>A paginated list of categories.</returns>
    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<CategoryModelPagination> GetPaged([FromQuery] PaginationParamsFromQueryModel paginationParams)
    {
        try
        {
            var paginatedCategories = this.categoryService.GetCategoriesPaged(paginationParams);
            return this.Ok(paginatedCategories);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An error occurred while getting categories.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Adds a new category.
    /// </summary>
    /// <param name="value">The category input model.</param>
    /// <returns>The created category.</returns>
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post([FromBody] CategoryInputAddModel value)
    {
        try
        {
            if (value == null)
            {
                this.logger.LogWarning("Category input is null.");
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Invalid model state for category input.");
                return this.BadRequest(this.ModelState);
            }

            var categoryModel = await this.categoryService.AddAsync(new CategoryModel
            {
                Name = value.Name,
                Description = value.Description,
            });

            return this.CreatedAtAction(nameof(this.Get), new { id = categoryModel.Id }, categoryModel);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An error occurred while adding a category.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="value">The category input model.</param>
    /// <returns>No content if successful, or a not found result if the category does not exist.</returns>
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Put(int id, [FromBody] CategoryInputAddModel value)
    {
        try
        {
            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Invalid model state for updating category {CategoryId}.", id);
                return this.BadRequest(this.ModelState);
            }

            await this.categoryService.UpdateAsync(new CategoryModel
            {
                Id = id,
                Name = value?.Name!,
                Description = value?.Description!,
            });
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Category {CategoryId} not found for update.", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An error occurred while updating category.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>No content if successful, or a not found result if the category does not exist.</returns>
    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await this.categoryService.DeleteAsync(id);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Category {CategoryId} not found for deletion.", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An error occurred  while deleting category.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
