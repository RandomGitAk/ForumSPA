using AutoMapper;
using Microsoft.AspNetCore.Http;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CommentModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class CommentService : ICommentService
{
    private readonly IComment commentRepository;
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor httpContextAccessor;

    public CommentService(IComment commentRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        this.commentRepository = commentRepository;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<CommentWithUserModel> AddAsync(CommentModel model)
    {
        var comment = this.mapper.Map<Comment>(model);
        await this.commentRepository.AddAsync(comment);
        return this.mapper.Map<CommentWithUserModel>(comment);
    }

    public async Task DeleteAsync(int modelId)
    {
        var existComment = await this.commentRepository.GetByIdAsync(modelId);
        if (existComment == null)
        {
            throw new InvalidOperationException("Comment not found.");
        }

        await this.commentRepository.DeleteByIdAsync(modelId);
    }

    public async Task<IEnumerable<CommentModel>> GetAllAsync()
    {
        var comments = await this.commentRepository.GetAllAsync();
        return this.mapper.Map<IEnumerable<CommentModel>>(comments);
    }

    public async Task<CommentModel> GetByIdAsync(int id)
    {
        var comment = await this.commentRepository.GetByIdAsync(id);
        return this.mapper.Map<CommentModel>(comment);
    }

    public async Task<CommentWithUserModel> GetByIdWithUserAsync(int id)
    {
        var comment = await this.commentRepository.GetByIdAsync(id);
        var commentWithUserModel = this.mapper.Map<CommentWithUserModel>(comment);
        var request = this.httpContextAccessor.HttpContext!.Request;
        commentWithUserModel.User.Image = $"{request.Scheme}://{request.Host}/{comment!.User!.Image}";
        return commentWithUserModel;
    }

    public async Task<IEnumerable<CommentWithUserModel>> GetByPostIdWithUserAsync(int postId)
    {
        var comments = await this.commentRepository.GetByPostIdAsync(postId);
        var commentsWithUserModel = this.mapper.Map<IEnumerable<CommentWithUserModel>>(comments);
        foreach (var comment in commentsWithUserModel)
        {
            var request = this.httpContextAccessor.HttpContext!.Request;
            comment.User.Image = $"{request.Scheme}://{request.Host}/{comment.User.Image}";

            SetUserImagePathsForReplies(comment.Replies, request);
        }

        return commentsWithUserModel;
    }

    public async Task UpdateAsync(CommentModel model)
    {
        var existComment = await this.commentRepository.GetByIdAsync(model?.Id ?? 0);
        if (existComment == null)
        {
            throw new InvalidOperationException("Comment not found.");
        }

        var comment = this.mapper.Map<Comment>(model);
        await this.commentRepository.UpdateAsync(comment);
    }

    private static void SetUserImagePathsForReplies(IEnumerable<CommentWithUserModel> replies, HttpRequest request)
    {
        foreach (var reply in replies)
        {
            reply.User.Image = $"{request.Scheme}://{request.Host}/{reply.User.Image}";

            if (reply.Replies != null && reply.Replies.Any())
            {
                SetUserImagePathsForReplies(reply.Replies, request);
            }
        }
    }
}
