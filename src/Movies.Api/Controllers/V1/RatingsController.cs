using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpGet(ApiEndpoints.V1.Movies.Rate)]
    public async Task<IActionResult> RateMovie([FromRoute] Guid movieId, [FromBody] RateMovieRequest request, CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.RateMovieAsync(movieId, request.Rating,  userId!.Value, token);
        
        return result ? Ok() : NotFound();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.V1.Movies.Rate)]
    public async Task<IActionResult> DeleteRating([FromRoute] Guid movieId, CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.DeleteRatingAsync(movieId, userId!.Value, token);

        return result ? Ok() : NotFound();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.V1.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatings(CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();
        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);
        var ratingsResponse = ratings.MapToResponse();

        return Ok(ratingsResponse);
    }
}