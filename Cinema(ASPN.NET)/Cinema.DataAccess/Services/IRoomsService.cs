using ELTE.Cinema.DataAccess.Models;

namespace ELTE.Cinema.DataAccess.Services;

public interface IRoomsService
{
    Task<IReadOnlyCollection<Room>> GetAllAsync();
    Task<Room> GetByIdAsync(int id);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
}