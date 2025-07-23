using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.Shared.Models;

namespace ELTE.TravelAgency.Admin.Persistence
{
    /// <summary>
    /// PErziszetencia felülete.
    /// </summary>
    public interface ITravelAgencyPersistence
    {
        /// <summary>
        /// Épületek olvasása.
        /// </summary>
        Task<IEnumerable<BuildingDto>> ReadBuildingsAsync();

        /// <summary>
        /// Városok olvasása.
        /// </summary>
        Task<IEnumerable<CityDto>> ReadCitiesAsync();

        /// <summary>
        /// Épület létrehozása.
        /// </summary>
        /// <param name="building">Épület.</param>
        Task<Boolean> CreateBuildingAsync(BuildingDto building);

        /// <summary>
        /// Épület módosítása.
        /// </summary>
        /// <param name="building">Épület.</param>
        Task<Boolean> UpdateBuildingAsync(BuildingDto building);

        /// <summary>
        /// Épület törlése.
        /// </summary>
        /// <param name="building">Épület.</param>
        Task<Boolean> DeleteBuildingAsync(BuildingDto building);

        /// <summary>
        /// Épületkép létrehozása.
        /// </summary>
        /// <param name="image">Épületkép.</param>
        Task<Boolean> CreateBuildingImageAsync(ImageDto image);

        /// <summary>
        /// Épületkép törlése.
        /// </summary>
        /// <param name="image">Épületkép.</param>
        Task<Boolean> DeleteBuildingImageAsync(ImageDto image);

        /// <summary>
        /// Bejelentkezés.
        /// </summary>
        /// <param name="email">Email cím.</param>
        /// <param name="password">Jelszó.</param>
        Task<Boolean> LoginAsync(string email, string password);

        /// <summary>
        /// Bejelentkezés hozzáférési tokenért.
        /// </summary>
        /// <param name="email">Email cím.</param>
        /// <param name="password">Jelszó.</param>
        Task<Boolean> LoginTokenAsync(string email, string password);

        /// <summary>
        /// Kijelentkezés.
        /// </summary>
        Task<Boolean> LogoutAsync();
    }
}
