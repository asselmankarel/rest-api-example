using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Genres = request.Genres.ToList(),
            YearOfRelease = request.YearOfRelease
        };
        
        return movie;
    }

    public static MovieResponse MapToResponse(this Movie movie)
    {
        return new MovieResponse()
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            YearOfRelease = movie.YearOfRelease,
            Rating = movie.Rating,
            Genres = movie.Genres,
        };
    }

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies)
    {
        return new MoviesResponse
        {
            Items = movies.Select(x => MapToResponse(x))
        };
    }
    
    public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
    {
        var movie = new Movie
        {
            Id = id,
            Title = request.Title,
            Genres = request.Genres.ToList(),
            YearOfRelease = request.YearOfRelease,
        };
        
        return movie;
    }

    public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> rating)
    {
        return rating.Select(x => new MovieRatingResponse
        {
            Rating = x.Rating,
            MovieId = x.MovieId,
            Slug = x.Slug,
        });
    }
}