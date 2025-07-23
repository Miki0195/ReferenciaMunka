using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.Shared.Models;

namespace ELTE.TravelAgency.Admin.Model
{
    /// <summary>
    /// Utazási ügynökség modell felülete.
    /// </summary>
    public interface ITravelAgencyModel
    {
        /// <summary>
        /// Városok lekérdezése.
        /// </summary>
        IReadOnlyList<CityDto> Cities { get; }

        /// <summary>
        /// Épületek lekérdezése.
        /// </summary>
        IReadOnlyList<BuildingDto> Buildings { get; }

        /// <summary>
        /// Bejelentkezettség lekérdezése.
        /// </summary>
        Boolean IsUserLoggedIn { get; }

        /// <summary>
        /// Épületváltozás eseménye.
        /// </summary>
        event EventHandler<BuildingEventArgs> BuildingChanged;

        /// <summary>
        /// Épület létrehozása.
        /// </summary>
        /// <param name="building">Az épület.</param>
        void CreateBuilding(BuildingDto building);

        /// <summary>
        /// Kép létrehozása.
        /// </summary>
        /// <param name="buildingId">Épület azonosító.</param>
        /// <param name="imageSmall">Kis kép.</param>
        /// <param name="imageLarge">Nagy kép.</param>
        void CreateImage(int buildingId, Byte[] imageSmall, Byte[] imageLarge);

        /// <summary>
        /// Épület módosítása.
        /// </summary>
        /// <param name="building">Az épület.</param>
        void UpdateBuilding(BuildingDto building);

        /// <summary>
        /// Épület törlése.
        /// </summary>
        /// <param name="building">Az épület.</param>
        void DeleteBuilding(BuildingDto building);

        /// <summary>
        /// Kép törlése.
        /// </summary>
        /// <param name="image">A kép.</param>
        void DeleteImage(ImageDto image);

        /// <summary>
        /// Adatok betöltése.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Adatok mentése.
        /// </summary>
        Task SaveAsync();

        /// <summary>
        /// Bejelentkezés.
        /// </summary>
        /// <param name="userName">Felhasználónév.</param>
        /// <param name="userPassword">Jelszó.</param>
        /// <param name="useCookies">Süti alapú authentikáció használata.</param>
        Task<Boolean> LoginAsync(string userName, string userPassword, bool useCookies = true);

        /// <summary>
        /// Kijelentkezés.
        /// </summary>
        Task<Boolean> LogoutAsync();
    }
}
