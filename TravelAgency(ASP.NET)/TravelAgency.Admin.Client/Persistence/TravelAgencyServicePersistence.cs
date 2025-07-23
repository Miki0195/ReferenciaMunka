using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ELTE.TravelAgency.Shared.Models;

namespace ELTE.TravelAgency.Admin.Persistence
{
    /// <summary>
    /// Szolgáltatás alapú perzisztencia.
    /// </summary>
    public class TravelAgencyServicePersistence : ITravelAgencyPersistence
    {
        private HttpClient _client;

        /// <summary>
        /// Szolgáltatás alapú perszisztencia példányosítása.
        /// </summary>
        /// <param name="baseAddress">A szolgáltatás címe.</param>
        public TravelAgencyServicePersistence(string baseAddress)
        {
            _client = new HttpClient(); // a szolgáltatás kliense
            _client.BaseAddress = new Uri(baseAddress); // megadjuk neki a címet
        }

        /// <summary>
        /// Szolgáltatás alapú perszisztencia példányosítása.
        /// </summary>
        /// <remarks>Integrációs teszteléshez.</remarks>
        /// <param name="client">A HTTP kliens objektum.</param>
        public TravelAgencyServicePersistence(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Épületek betöltése.
        /// </summary>
        public async Task<IEnumerable<BuildingDto>> ReadBuildingsAsync()
        {
            try
            {
                // a kéréseket a kliensen keresztül végezzük
                HttpResponseMessage response = await _client.GetAsync("api/buildings/");
                if (response.IsSuccessStatusCode) // amennyiben sikeres a művelet
                {
                    IEnumerable<BuildingDto> buildings = await response.Content.ReadAsAsync<IEnumerable<BuildingDto>>();
                    // a tartalmat JSON formátumból objektumokká alakítjuk

                    // képek lekérdezése:
                    foreach (BuildingDto building in buildings)
                    {
                        response = await _client.GetAsync($"api/buildings/{building.Id}/images");
                        if (response.IsSuccessStatusCode)
                        {
                            building.Images = (await response.Content.ReadAsAsync<IEnumerable<ImageDto>>()).ToList();
                        }
                    }

                    return buildings;
                }
                else
                {
                    throw new PersistenceUnavailableException("Service returned response: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }

        }

        /// <summary>
        /// Városok betöltése.
        /// </summary>
        public async Task<IEnumerable<CityDto>> ReadCitiesAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/cities/");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<IEnumerable<CityDto>>();
                }
                else
                {
                    throw new PersistenceUnavailableException("Service returned response: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }

        /// <summary>
        /// Épület létrehozása.
        /// </summary>
        /// <param name="building">Épület.</param>
        public async Task<Boolean> CreateBuildingAsync(BuildingDto building)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/buildings/", building); // az értékeket azonnal JSON formátumra alakítjuk
                if (response.IsSuccessStatusCode)
                {
                    building.Id = (await response.Content.ReadAsAsync<BuildingDto>()).Id; // a válaszüzenetben megkapjuk a végleges azonosítót
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }

        /// <summary>
        /// Épület módosítása.
        /// </summary>
        /// <param name="building">Épület.</param>
        public async Task<Boolean> UpdateBuildingAsync(BuildingDto building)
        {
            try
            {
                HttpResponseMessage response = await _client.PutAsJsonAsync("api/buildings/", building);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }

        /// <summary>
        /// Épület törlése.
        /// </summary>
        /// <param name="building">Épület.</param>
        public async Task<Boolean> DeleteBuildingAsync(BuildingDto building)
        {
            try
            {
                HttpResponseMessage response = await _client.DeleteAsync("api/buildings/" + building.Id);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }

        /// <summary>
        /// Épületkép létrehozása.
        /// </summary>
        /// <param name="image">Épületkép.</param>
        public async Task<Boolean> CreateBuildingImageAsync(ImageDto image)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/buildings/images/", image); // elküldjük a képet
                if (response.IsSuccessStatusCode)
                {
                    image.Id = await response.Content.ReadAsAsync<int>(); // a válaszüzenetben megkapjuk az azonosítót
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }

        /// <summary>
        /// Épületkép törlése.
        /// </summary>
        /// <param name="image">Épületkép.</param>
        public async Task<Boolean> DeleteBuildingImageAsync(ImageDto image)
        {
            try
            {
                HttpResponseMessage response = await _client.DeleteAsync("api/buildings/images/" + image.Id);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }


        /// <summary>
        /// Bejelentkezés.
        /// </summary>
        /// <param name="email">Email cím.</param>
        /// <param name="password">Jelszó.</param>
        public async Task<Boolean> LoginAsync(string email, string password)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/users/login", new LoginDto
                {
                    Email = email,
                    Password = password
                });
                return response.IsSuccessStatusCode; // a művelet eredménye megadja a bejelentkezés sikerességét
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }


        /// <summary>
        /// Bejelentkezés hozzáférési tokenért.
        /// </summary>
        /// <param name="email">Email cím.</param>
        /// <param name="password">Jelszó.</param>
        public async Task<Boolean> LoginTokenAsync(string email, string password)
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/users/login-token", new LoginDto
                {
                    Email = email,
                    Password = password
                });

                if (response.IsSuccessStatusCode && 
                    response.Headers.TryGetValues("X-Access-Token", out var values))
                {
                    string? token = values.FirstOrDefault();
                    if (token != null)
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return response.IsSuccessStatusCode; // a művelet eredménye megadja a bejelentkezés sikerességét
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }

        /// <summary>
        /// Kijelentkezés.
        /// </summary>
        public async Task<Boolean> LogoutAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/users/logout");
                return !response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new PersistenceUnavailableException(ex);
            }
        }
    }
}
