// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Movies.Api.Sdk;
using Movies.Contracts.Requests;
using Refit;

var moviesApi = RestService.For<IMoviesApi>("htts://localhost:5001");
var movie =  await moviesApi.GetMovieAsync("");

Console.WriteLine(JsonSerializer.Serialize(movie));

var request = new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 5,
};
var movies = await moviesApi.GetMoviesAsync(request);
Console.WriteLine(JsonSerializer.Serialize(movies));