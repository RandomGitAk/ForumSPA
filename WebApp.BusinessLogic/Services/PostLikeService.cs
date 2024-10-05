using AutoMapper;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.PostLikeModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class PostLikeService : IPostLikeService
{
    private readonly IPostLike likeRepository;
    private readonly IMapper mapper;

    public PostLikeService(IPostLike likeRepository, IMapper mapper)
    {
        this.likeRepository = likeRepository;
        this.mapper = mapper;
    }

    public async Task<PostLikeModel> AddAsync(PostLikeModel model)
    {
        var existLike = await this.likeRepository.GetLikeByPostAndUserIdAsyn(model?.PostId ?? 0, model?.UserId ?? 0);
        if (existLike != null)
        {
            existLike.IsLike = model!.IsLike;
            await this.likeRepository.UpdateAsync(existLike);
            return this.mapper.Map<PostLikeModel>(existLike);
        }
        else
        {
            var like = this.mapper.Map<PostLike>(model);
            await this.likeRepository.AddAsync(like);
            return this.mapper.Map<PostLikeModel>(like);
        }
    }

    public async Task DeleteAsync(int userId, int postId)
    {
        var existLike = await this.likeRepository.GetLikeByPostAndUserIdAsyn(postId, userId);
        if (existLike == null)
        {
            throw new InvalidOperationException("Like not found.");
        }

        this.likeRepository.Delete(existLike);
    }

    public async Task<PostLikeModel> GetByIdAsync(int userId, int postId)
    {
        var like = await this.likeRepository.GetLikeByPostAndUserIdAsyn(postId, userId);
        return this.mapper.Map<PostLikeModel>(like);
    }
}
