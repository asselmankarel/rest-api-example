using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    
    public string Slug => GenerateSlug();
    
    public float? Rating { get; set; }
    
    public int? UserRating { get; set; }
    public int YearOfRelease { get; set; }
    public required List<string> Genres { get; set; } = new();
    
    private string GenerateSlug()
    {
        var sluggedTitle = SlugRegexp().Replace(Title, string.Empty).ToLower().Replace(" ", "-");
        return $"{sluggedTitle}-{YearOfRelease}";
    }

    [GeneratedRegex(@"[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 10)]
    private static partial Regex SlugRegexp();
}