using AutoMapper;
using WebApp.BusinessLogic.Models.CategoryModels;
using WebApp.BusinessLogic.Models.CommentLikeModels;
using WebApp.BusinessLogic.Models.CommentModels;
using WebApp.BusinessLogic.Models.PostLikeModels;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models.RoleModels;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;

namespace WebApp.BusinessLogic;
public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        _ = this.CreateMap<UserModel, User>()
                   .ForMember(dest => dest.HashedPasssword, opt => opt.Ignore())
                   .ForMember(dest => dest.Salt, opt => opt.Ignore())
                   .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Role != null ? src.Role.Id : 0));

        _ = this.CreateMap<User, UserModel>();

        _ = this.CreateMap<RoleModel, Role>();

        _ = this.CreateMap<Role, RoleModel>();

        _ = this.CreateMap<Category, CategoryModel>()
           .ReverseMap();

        _ = this.CreateMap<PagedList<Category>, CategoryModelPagination>()
         .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
         .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalItems))
         .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.CurrentPage))
         .ForMember(dest => dest.PerPage, opt => opt.MapFrom(src => src.PageSize))
         .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));

        _ = this.CreateMap<Post, PostModel>()
           .ForMember(pm => pm.CountLikes, p => p.MapFrom(x => x.Likes!.Count(l => l.IsLike) - x.Likes!.Count(l => !l.IsLike)))
           .ForMember(pm => pm.CountComments, p => p.MapFrom(x => x.Comments!.Count()))
           .ForMember(pm => pm.CategoryName, p => p.MapFrom(x => x.Category!.Name))
           .ForMember(pm => pm.User, p => p.MapFrom(x => x.User))
           .ReverseMap();

        _ = this.CreateMap<Post, PostWithDetailsModel>()
           .IncludeBase<Post, PostModel>()
           .ForMember(pm => pm.UserReaction, p => p.MapFrom((src, dest, destMember, context) =>
           {
               var userId = (int)context.Items["UserId"];
               var userLike = src.Likes!.FirstOrDefault(l => l.UserId == userId);

               if (userLike == null)
               {
                   return "None";
               }

               return userLike.IsLike ? "Like" : "Dislike";
           }));

        _ = this.CreateMap<CreatePostModel, PostModel>()
         .ReverseMap();

        _ = this.CreateMap<Post, CreatePostModel>()
         .ReverseMap();

        _ = this.CreateMap<PagedList<Post>, PostModelPagination>()
           .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
           .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalItems))
           .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.CurrentPage))
           .ForMember(dest => dest.PerPage, opt => opt.MapFrom(src => src.PageSize))
           .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));

        _ = this.CreateMap<Comment, CommentModel>()
         .ReverseMap();

        _ = this.CreateMap<Comment, CommentWithUserModel>()
           .ForMember(pm => pm.CountLikes, p => p.MapFrom(x => x.Likes!.Count()))
           .ReverseMap();

        _ = this.CreateMap<User, UserRegisterModel>()
         .ReverseMap();

        _ = this.CreateMap<PagedList<User>, UserModelPagination>()
          .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
          .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalItems))
          .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.CurrentPage))
          .ForMember(dest => dest.PerPage, opt => opt.MapFrom(src => src.PageSize))
          .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));

        _ = this.CreateMap<PostLike, PostLikeModel>()
         .ReverseMap();

        _ = this.CreateMap<CommentLike, CommentLikeModel>()
          .ReverseMap();
    }
}
