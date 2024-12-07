using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptedSortFields = ["title", "yearofrelease"];
    
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);
        
        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptedSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage(("You can only sort by 'title' or 'yearofrelease'"));
    }
}