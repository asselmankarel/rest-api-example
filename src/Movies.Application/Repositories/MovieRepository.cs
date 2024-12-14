using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
   private readonly IDbConnectionFactory _connectionFactory;

   public MovieRepository(IDbConnectionFactory connectionFactory)
   {
       _connectionFactory = connectionFactory;
   }

   public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
   {
       using var connection = await _connectionFactory.CreateConnectionAsync(token);
       using var transaction = connection.BeginTransaction();
       var result = await connection.ExecuteAsync(
           new CommandDefinition("""
                                    insert into movies (id, slug, title, yearOfReleas)
                                    values (@Id, @Slug, @Title, @YearOfRelease)
                                """, movie, cancellationToken: token));
       if (result > 1)
       {
           foreach (var genre in movie.Genres)
           {
               await connection.ExecuteAsync(
                   new CommandDefinition("""
                                         insert into genres (movieId, name)
                                         values (@MovieId, @Name)
                                         """, new { MovidId = movie.Id, Name = genre }, cancellationToken: token));
           }
       }
       
       transaction.Commit();
       
       return result > 0;
   }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                select m.*, round(avg(r.rating),1) as rating, myr.rating as userRating from movies m
                left join ratings r on r.movieId = m.Id
                left join ratings myr on myr.movieId = m.Id
                    and myr.userId = @userId
                where m.id = @id
                group by id, userRating
                """, new { id, userId }, cancellationToken: token));
        
        if (movie is null) return null;
        
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("select name from genres where movieId = @id", new { id }));
        
        foreach (var genre in genres)
            movie.Genres.Add(genre);
        
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                select m.*, round(avg(r.rating),1) as rating, myr.rating as userRating from movies m
                left join ratings r on r.movieId = m.Id
                left join ratings myr on myr.movieId = m.Id
                    and myr.userId = @userId
                where slug = @slug
                group by id, userRating
                """,
                new { slug, userId }, cancellationToken: token));
        
        if (movie is null) return null;
        
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("select name from genres where movieId = @id", new { id = movie.Id }, cancellationToken: token));
        
        foreach (var genre in genres)
            movie.Genres.Add(genre);
        
        return movie;

    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(
            new CommandDefinition("delete from genres where movieId = @id", new { id = movie.Id }, cancellationToken: token));
        
        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition("""
                                      insert into genres (movieId, name)
                                      values (@MovieId, @Name)
                                      """, new { MovidId = movie.Id, Name = genre }, cancellationToken: token));
        }

        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  update movies set slug = @Slug, title = @Title, yearOfRelease = @YearOfRelease
                                  where id = @Id
                                  """, movie, cancellationToken: token));
        
        transaction.Commit();
        
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(
            new CommandDefinition("delete from genres where movieId = @id", new { id }, cancellationToken: token));

        var result = await connection.ExecuteAsync(
            new CommandDefinition("delete from movies where id = @id", new { id }, cancellationToken: token));
        
        transaction.Commit();
        
        return result > 0;
    }   

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);

        var orderClause = string.Empty;
        if (options.SortField != null)
        {
            orderClause = $"""
                           , m.{options.SortField}
                           order by m.{options.SortOrder} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                           """;
        }
        
        var result = await connection.QueryAsync(
            new CommandDefinition($"""
                  select m.*, string_agg(distinct g.name, ',') as genres,
                         round(avg(r.rating),1) as rating, myr.rating as userRating
                  from movies m
                  left join genres g on m.id = g.movieId
                  left join ratings r on r.movieId = m.Id
                  left join ratings myr on myr.movieId = m.Id and myr.userId = @UserId
                  where (@title is null or m.title like ('%' || @Title || '%'))
                  and (@yearOfRelease is null or m.yearOfRelease = @YearOfRelease)
                  group by id, userRating {orderClause}
                  Limit @pageSize
                  offset @pageOffset
                  """, new 
                { 
                    options.UserId,
                    options.YearOfRelease,
                    options.Title,
                    options.PageSize, 
                    pageOffset = (options.Page -1) * options.PageSize
                    
                }, cancellationToken: token
            ));
        
        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearOfRelease,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userRating,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("select count(1) from movies where id = @id", new { id }, cancellationToken: token));
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<int>(new CommandDefinition("""
                    select count(id) from movies
                    where (@title is null or m.title like ('%' || @Title || '%'))
                    and (@yearOfRelease is null or m.yearOfRelease = @YearOfRelease)    
                """, new { title, year = yearOfRelease }, cancellationToken: token));
    }
}