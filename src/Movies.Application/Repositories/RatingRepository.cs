using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RatingRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                insert into ratings (userId, movieId, rating)
                values (@userId, @movieId, @rating)
                on conflict (userId, movieId) do update
                    set rating = @rating
            """, new { userId, movieId, rating }, cancellationToken: token));
        
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition(
            """
            SELECT round(avg(r.rating),1) from Ratings r
            where r.movie_id == movieId
            """, new { movieId }, cancellationToken: token));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
            SELECT round(avg(r.rating),1), 
            (SELECT rating from ratings where userId == @userId and movieId = @movieId limit 1)
            from Ratings r
            where r.movie_id == movieId
            """, new { movieId, userId }, cancellationToken: token));
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
                delete from rating where movieId = @movieId and userId = @userId
            """, new { movieId, userId }, cancellationToken: token));

        return result > 0;
    }
}