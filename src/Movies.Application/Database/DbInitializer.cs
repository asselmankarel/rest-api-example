﻿using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync("""
                                      create table if not exists movies(
                                      id UUID primary key,
                                      slug TEXT not null,
                                      title TEXT not null,
                                      yearOfRelease integer not null);
                                      """);
        
        await connection.ExecuteAsync("""
                                      create unique index concurrently if not exists movies_slug_idx
                                      on movies
                                      using btree(slug);
                                      """);
        
        await connection.ExecuteAsync("""
                                      create table if not exists genres(
                                          movieId UUID references movies (id),
                                          name TEXT not null);
                                      """);
        
        await connection.ExecuteAsync("""
                                      create table if not exists ratings(
                                          userId UUID ,
                                          movieId UUID references movies (id),
                                          rating int not null,
                                          primary key (userId, movieId));
                                      """);
    }
}