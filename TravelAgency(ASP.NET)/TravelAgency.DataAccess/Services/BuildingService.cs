using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.DataAccess.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly TravelAgencyContext _context;

        public BuildingService(TravelAgencyContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<Building>> GetAllAsync()
        {
            return await _context.Buildings
                .Include(b => b.City)
                .ToListAsync();
        }

        public async Task<Building> GetAsync(int id)
        {
            var building = await _context.Buildings
                .Include(b => b.City)
                .SingleAsync(b=> b.Id == id);

            return building;
        }

        public async Task<Building> GetWithApartments(int id)
        {
            return await _context.Buildings
                .Include(b => b.City)
                .Include(b => b.Apartments)
                .SingleAsync(b => b.Id == id);
        }

        public async Task<IReadOnlyCollection<Building>> GetByCityAsync(int cityId)
        {
            return await _context.Buildings
                .Where(building => building.CityId == cityId)
                .Include(b => b.City)
                .ToListAsync();
        }

        public async Task<ICollection<int>> GetImageIdsAsync(int id)
        {
            return await _context.BuildingImages
                .Where(image => image.BuildingId == id)
                .Select(image => image.Id)
                .ToListAsync();
        }
        
        public async Task<List<BuildingImage>> GetImagesAsync(int id)
        {
            return await _context.BuildingImages
                .Where(image => image.BuildingId == id)
                .ToListAsync();
        }


        public async Task<byte[]?> GetMainImageAsync(int id, bool large)
        {
            // lekérjük az épület első tárolt képét (kicsiben)
            
            return await _context.BuildingImages
                .Where(image => image.BuildingId == id)
                .Select(image => large ? image.ImageLarge : image.ImageSmall)
                .FirstOrDefaultAsync();
        }

        public async Task<byte[]> GetImageAsync(int imageId, bool large)
        {
            // lekérjük a képet a kért méretben (kicsi, nagy)
            Byte[]? imageContent = await _context.BuildingImages
                .Where(image => image.Id == imageId)
                .Select(image => large ? image.ImageLarge : image.ImageSmall)
                .SingleAsync();

            // Amennyiben a kép a megadott azonosítóval nem létezett, null-lal térünk vissza
            return imageContent;
        }

        public async Task AddAsync(Building building)
        {
            await _context.Buildings.AddAsync(building);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Building building)
        {
            if (!await _context.Buildings.AnyAsync(r => r.Id == building.Id))
                throw new ArgumentException("Building does not exist.", nameof(building));

            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var building = await GetAsync(id);
            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
        }

        public async Task AddImageAsync(BuildingImage image)
        {
            await _context.BuildingImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(int imageId)
        {
            var image = await _context.BuildingImages.SingleAsync(bi => bi.Id == imageId);
            _context.BuildingImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }
}
