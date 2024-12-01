using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;

    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;

        RuleFor(m => m.Id).NotEmpty();
        RuleFor(m => m.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(m => m.Genres).NotEmpty().WithMessage("1 Genre is required");
        RuleFor(m => m.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);
        RuleFor(m => m.Slug).MustAsync(ValidateSlug).WithMessage("This movie already exists in the system");
        
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token = default)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug, default, token);
        if (existingMovie is not null)
        {
            return existingMovie.Id == movie.Id;
        }
        
        return existingMovie is null;
    }
}