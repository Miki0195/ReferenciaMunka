using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;

namespace ELTE.TravelAgency.DataAccess.Services;

public interface IBuildingService
{
    Task<IReadOnlyCollection<Building>> GetAllAsync();
    Task<Building> GetAsync(int id);
    Task<Building> GetWithApartments(int id);
    Task<IReadOnlyCollection<Building>> GetByCityAsync(int cityId);
    Task<ICollection<int>> GetImageIdsAsync(int id);
    Task<List<BuildingImage>> GetImagesAsync(int id);
    Task<byte[]?> GetMainImageAsync(int id, bool large = false);
    Task<byte[]> GetImageAsync(int imageId, bool large);
    Task AddAsync(Building building);
    Task UpdateAsync(Building building);
    Task DeleteAsync(int id);
    Task AddImageAsync(BuildingImage image);
    Task DeleteImageAsync(int imageId);
}