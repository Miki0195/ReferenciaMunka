using ELTE.Cinema.DataAccess.Models;

namespace ELTE.Cinema.DataAccess.Services;

public interface IMoviesService
{
    Task<IReadOnlyCollection<Movie>> GetLatestMoviesAsync(int? count = null);
    Task<Movie> GetByIdAsync(int id);
    Task AddAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(int id);
}