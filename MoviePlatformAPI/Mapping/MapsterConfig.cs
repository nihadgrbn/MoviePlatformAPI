using Mapster;
using MoviePlatformAPI.DTOs.Comments;
using MoviePlatformAPI.DTOs.Movies;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Mapping;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Movie, MovieResponseDto>()
            .Map(dest => dest.OwnerUsername, src => src.Owner != null ? src.Owner.Username : string.Empty);

        config.NewConfig<MovieCreateDto, Movie>();
        config.NewConfig<Comment, CommentResponseDto>()
            .Map(dest => dest.AuthorId, src => src.UserId)
            .Map(dest => dest.AuthorUsername, src => src.User != null ? src.User.Username : string.Empty);
    }
}