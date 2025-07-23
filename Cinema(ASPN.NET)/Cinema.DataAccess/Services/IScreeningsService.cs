using ELTE.Cinema.DataAccess.Models;

namespace ELTE.Cinema.DataAccess.Services;

public interface IScreeningsService
{
    Task<IReadOnlyCollection<Screening>> GetForDateAsync(DateTime date);
    Task<Screening> GetByIdAsync(int id);
    Task<PaginatedResult<Screening>> GetAllAsync(int? movieId = null, int? roomId = null, int? page = null, int? pageSize = null, DateTime? from = null, DateTime? until = null);
    Task AddAsync(Screening room);
    Task UpdateAsync(Screening room);
    Task DeleteAsync(int id);
    Task<List<Seat>> GetSeatsByScreeningAsync(int id);
    Task SellSeatForScreeningAsync(int id, Seat seat);
}