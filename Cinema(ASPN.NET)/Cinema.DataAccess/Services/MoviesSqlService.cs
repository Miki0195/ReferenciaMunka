using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.DataAccess.Services;

// FromSql is safe against sql injections
// https://learn.microsoft.com/en-us/ef/core/querying/sql-queries?tabs=sqlserver#passing-parameters

internal class MoviesSqlService : IMoviesService
{
    private readonly CinemaDbContext _context;

    public MoviesSqlService(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Movie>> GetLatestMoviesAsync(int? count = null)
    {
        if (count is null)
        {
            return await _context.Movies.FromSql(
                $"""
                 SELECT
                 Id, Title, Year, Director, Synopsis, Length, Image, CreatedAt, DeletedAt
                 FROM Movies
                 WHERE DeletedAt is null
                 ORDER BY CreatedAt DESC
                 """
            ).ToListAsync();
        }
        
        return await _context.Movies.FromSql(
            $"""
             SELECT TOP ({count.Value})
             Id, Title, Year, Director, Synopsis, Length, Image, CreatedAt, DeletedAt
             FROM Movies
             WHERE DeletedAt is null
             ORDER BY CreatedAt DESC
             """
        ).ToListAsync();
    }

    public async Task<Movie> GetByIdAsync(int id)
    {
        var movie = await _context.Movies
            .FromSql(
                $"""
                   SELECT *    
                   FROM movies
                   WHERE Id = {id}
                   AND DeletedAt is null
                 """)
            .FirstOrDefaultAsync();

        if (movie is null)
            throw new EntityNotFoundException(nameof(Movie));

        return movie;
    }

    public Task AddAsync(Movie movie)
    {
        var newId = _context.Database.SqlQuery<int>(
                $"""
                  INSERT INTO Movies (Title, Year, Director, Synopsis, Length, Image, CreatedAt)
                  OUTPUT INSERTED.Id
                  VALUES (
                     {movie.Title}, {movie.Year}, {movie.Director}, {movie.Synopsis}, {movie.Length}, {movie.Image}, {movie.CreatedAt})
                 """)
            .AsEnumerable()
            .First();
            

        movie.Id = newId;

        return Task.CompletedTask;
    }

    public async Task UpdateAsync(Movie movie)
    {
        var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
                 UPDATE Movies
                 SET Title = {movie.Title},
                     Year = {movie.Year},
                     Director = {movie.Director},
                     Synopsis = {movie.Synopsis},
                     Length = {movie.Length},
                     Image = {movie.Image}
                 WHERE Id = {movie.Id}
             """);

        if (affectedRows == 0)
            throw new EntityNotFoundException(nameof(Movie));
    }

    public async Task DeleteAsync(int id)
    {
        var affectedRows = await _context.Database
            .ExecuteSqlInterpolatedAsync($"UPDATE Movies SET DeletedAt = {DateTime.UtcNow} WHERE Id = {id}");

        if (affectedRows == 0)
            throw new EntityNotFoundException(nameof(Movie));
    }
}