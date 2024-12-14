using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers.V2;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }
    
    [HttpGet(ApiEndpoints.V2.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken token = default)
    {
        var user = HttpContext.GetUserId();
        var movie = Guid.TryParse(idOrSlug, out var id) ? 
            await _movieService.GetByIdAsync(id, user, token) :
            await _movieService.GetBySlugAsync(idOrSlug, user, token);
        
        if (movie == null)
            return NotFound();
        
        return Ok(movie.MapToResponse());
    }
}