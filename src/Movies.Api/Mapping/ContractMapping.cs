﻿using Movies.Application.Models;
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

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies, int page, int pageSize, int totalCount)
    {
        return new MoviesResponse
        {
            Items = movies.Select(x => MapToResponse(x)),
            Page = page,
            PageSize = pageSize,
            Total = totalCount
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

    public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
    {
        return new GetAllMoviesOptions
        {
            Title = request.Title,
            YearOfRelease = request.Year,
            SortField = request.SortBy?.Trim('+','-'),
            SortOrder = request.SortBy is null ? SortOrder.Unsorted : 
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
    {
        options.UserId = userId;
        
        return options;
    }
}