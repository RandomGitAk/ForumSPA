using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class PostService : IPostService
{
    private readonly IPost postRepository;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMapper mapper;

    public PostService(IPost postRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        this.postRepository = postRepository;
        this.httpContextAccessor = httpContextAccessor;
        this.mapper = mapper;
    }

    public async Task<PostModel> AddAsync(CreatePostModel model)
    {
        var post = this.mapper.Map<Post>(model);
        await this.postRepository.AddAsync(post);
        return this.mapper.Map<PostModel>(post);
    }

    public async Task DeleteAsync(int modelId)
    {
        var existPost = await this.postRepository.GetByIdAsync(modelId);
        if (existPost == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        await this.postRepository.DeleteByIdAsync(modelId);
    }

    public async Task<IEnumerable<PostModel>> GetAllAsync()
    {
        var posts = await this.postRepository.GetAllAsync();
        return this.mapper.Map<IEnumerable<PostModel>>(posts);
    }

    public async Task<PostModel> GetByIdAsync(int id)
    {
        var post = await this.postRepository.GetByIdAsync(id);
        return this.mapper.Map<PostModel>(post);
    }

    public PostModelPagination GetAllPostsWithDetails(PaginationParamsFromQueryModel paginationParams)
    {
        var pageListPost = this.postRepository.GetAllPostsWithDetails(new QueryOptions
        {
            CurrentPage = (paginationParams?.Page ?? 0) + 1,
            PageSize = paginationParams?.PerPage ?? 0,
            SearchPropertyName = paginationParams?.SearchPropertyName!,
            SearchTerm = paginationParams?.SearchTerm!,
            CategoryId = paginationParams!.CategoryId == 0 ? null : paginationParams.CategoryId,
        });

        var postModelPagination = this.mapper.Map<PostModelPagination>(pageListPost);
        return postModelPagination;
    }

    public async Task UpdateAsync(CreatePostModel model)
    {
        var existPost = await this.postRepository.GetByIdAsync(model?.Id ?? 0);
        if (existPost == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        var post = this.mapper.Map<Post>(model);
        await this.postRepository.UpdateAsync(post);
    }

    public async Task<PostWithDetailsModel> GetPostWithDetailsByIdAsync(int id)
    {
        _ = int.TryParse(this.httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
        var post = await this.postRepository.GetPostWithDetailsByIdAsync(id);
        var postWithDetailsModel = this.mapper.Map<PostWithDetailsModel>(post, opt =>
        {
            opt.Items["UserId"] = userId;
        });
        postWithDetailsModel.User.Image = $"{this.httpContextAccessor.HttpContext!.Request.Scheme}://{this.httpContextAccessor.HttpContext!.Request.Host}/{postWithDetailsModel.User.Image}";
        return postWithDetailsModel;
    }

    public PostModelPagination GetPostsByUserId(PaginationParamsFromQueryModel paginationParams, int userId)
    {
        var pageListPost = this.postRepository.GetPostsByUserIdAsync(
            new QueryOptions
            {
                CurrentPage = (paginationParams?.Page ?? 0) + 1,
                PageSize = paginationParams?.PerPage ?? 0,
                SearchPropertyName = paginationParams?.SearchPropertyName!,
                SearchTerm = paginationParams?.SearchTerm!,
            }, userId);

        return this.mapper.Map<PostModelPagination>(pageListPost);
    }

    public async Task IncrementPostViews(int postId)
    {
        var existPost = await this.postRepository.GetByIdAsync(postId);
        if (existPost == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        existPost.Views++;
        await this.postRepository.UpdateAsync(existPost);
    }
}
