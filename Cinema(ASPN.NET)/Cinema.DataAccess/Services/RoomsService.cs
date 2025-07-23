using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.DataAccess.Services;

internal class RoomsService : IRoomsService
{
    private readonly CinemaDbContext _context;

    public RoomsService(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Room>> GetAllAsync()
    {
        return await _context.Rooms
            .Where(r => !r.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<Room> GetByIdAsync(int id)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id && !r.DeletedAt.HasValue);
        if (room is null)
            throw new EntityNotFoundException(nameof(Room));

        return room;
    }

    public async Task AddAsync(Room room)
    {
        await CheckIfNameExistsAsync(room);

        room.CreatedAt = DateTime.UtcNow;
        
        try
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to create new room.", ex);
        }
    }

    public async Task UpdateAsync(Room room)
    {
        var existingRoom = await GetByIdAsync(room.Id);
        
        if (existingRoom == null)
            throw new EntityNotFoundException(nameof(Room));

        await CheckIfNameExistsAsync(room);
        room.CreatedAt = existingRoom.CreatedAt;
        
        try
        {
            _context.Entry(existingRoom).State = EntityState.Detached;
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to update room.", ex);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var room = await GetByIdAsync(id);
        
        room.DeletedAt = DateTime.UtcNow;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to delete room.", ex);
        }
    }
    
    private async Task CheckIfNameExistsAsync(Room room)
    {
        if (await _context.Rooms.AnyAsync(r => r.Id != room.Id
                                                && !r.DeletedAt.HasValue 
                                                && r.Name.ToLower().Equals(room.Name.ToLower())))
            throw new InvalidDataException("Room with the same name already exists");
    }
}