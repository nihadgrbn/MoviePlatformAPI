//using Mapster;
//using MoviePlatformAPI.Models;
//using MoviePlatformAPI.DTOs.Comments;

//public class CommentMapping : IRegister
//{
//    public void Register(TypeAdapterConfig config)
//    {
//        config.NewConfig<Comment, CommentResponseDto>()
//            .Map(dest => dest.AuthorUsername, src => src.User != null ? src.User.Username : string.Empty);
//    }
//}