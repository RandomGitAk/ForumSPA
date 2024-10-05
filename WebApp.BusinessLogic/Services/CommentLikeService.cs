using AutoMapper;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CommentLikeModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class CommentLikeService : ICommentLikeService
{
    private readonly ICommentLike likeRepository;
    private readonly IMapper mapper;

    public CommentLikeService(ICommentLike likeRepository, IMapper mapper)
    {
        this.likeRepository = likeRepository;
        this.mapper = mapper;
    }

    public async Task<CommentLikeModel> AddAsync(CommentLikeModel model)
    {
        var existLike = await this.likeRepository.GetLikeByCommentAndUserIdAsyn(model?.CommentId ?? 0, model?.UserId ?? 0);
        if (existLike == null)
        {
            var like = this.mapper.Map<CommentLike>(model);
            await this.likeRepository.AddAsync(like);
            return this.mapper.Map<CommentLikeModel>(like);
        }

        return this.mapper.Map<CommentLikeModel>(existLike);
    }

    public async Task DeleteAsync(int userId, int commentId)
    {
        var existLike = await this.likeRepository.GetLikeByCommentAndUserIdAsyn(commentId, userId);
        if (existLike == null)
        {
            throw new InvalidOperationException("Like not found.");
        }

        this.likeRepository.Delete(existLike);
    }

    public async Task<CommentLikeModel> GetByIdAsync(int userId, int commentId)
    {
        var like = await this.likeRepository.GetLikeByCommentAndUserIdAsyn(commentId, userId);
        return this.mapper.Map<CommentLikeModel>(like);
    }
}
