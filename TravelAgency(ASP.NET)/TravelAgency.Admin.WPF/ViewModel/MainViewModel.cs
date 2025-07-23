using CommunityToolkit.Mvvm.Input;
using ELTE.TravelAgency.Admin.Model;
using ELTE.TravelAgency.Admin.Persistence;
using ELTE.TravelAgency.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// A nézetmodell típusa.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ITravelAgencyModel _model;
        private ObservableCollection<BuildingDto> _buildings = new();
        private ObservableCollection<CityDto> _cities = new();
        private BuildingDto? _selectedBuilding;
        private Boolean _isLoaded;

        /// <summary>
        /// Épületek lekérdezése.
        /// </summary>
        public ObservableCollection<BuildingDto> Buildings
        {
            get { return _buildings; }
            private set
            {
                if (_buildings != value)
                {
                    _buildings = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Épületek lekérdezése.
        /// </summary>
        public ObservableCollection<CityDto> Cities
        {
            get { return _cities; }
            private set
            {
                if (_cities != value)
                {
                    _cities = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Betöltöttség lekérdezése.
        /// </summary>
        public Boolean IsLoaded
        {
            get { return _isLoaded; }
            private set
            {
                if (_isLoaded != value)
                {
                    _isLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Kijelölt épület lekérdezése, vagy beállítása.
        /// </summary>
        public BuildingDto? SelectedBuilding 
        { 
            get { return _selectedBuilding; }
            set
            {
                if (_selectedBuilding != value)
                {
                    _selectedBuilding = value;
                    OnPropertyChanged();
                }
            } 
        }

        /// <summary>
        /// Szerkesztett épület lekérdezése.
        /// </summary>
        public BuildingDto? EditedBuilding { get; private set; }

        /// <summary>
        /// Épület törlés parancsának lekérdezése.
        /// </summary>
        public RelayCommand CreateBuildingCommand { get; private set; }

        /// <summary>
        /// Módosítás parancsának lekérdezése.
        /// </summary>
        public RelayCommand<BuildingDto> UpdateBuildingCommand { get; private set; }

        /// <summary>
        /// Épület törlés parancsának lekérdezése.
        /// </summary>
        public RelayCommand<BuildingDto> DeleteBuildingCommand { get; private set; }

        /// <summary>
        /// Kép hozzáadás parancsának lekérdezése.
        /// </summary>
        public RelayCommand<BuildingDto> CreateImageCommand { get; private set; }

        /// <summary>
        /// Kép törlés parancsának lekérdezése.
        /// </summary>
        public RelayCommand<ImageDto> DeleteImageCommand { get; private set; }

        /// <summary>
        /// Változtatások mentése parancsának lekérdezése.
        /// </summary>
        public RelayCommand SaveChangesCommand { get; private set; }

        /// <summary>
        /// Változtatások elvetése parancsának lekérdezése.
        /// </summary>
        public RelayCommand CancelChangesCommand { get; private set; }

        /// <summary>
        /// Kilépés parancsának lekérdezése.
        /// </summary>
        public RelayCommand ExitCommand { get; private set; }

        /// <summary>
        /// Betöltés parancsának lekérdezése.
        /// </summary>
        public RelayCommand LoadCommand { get; private set; }

        /// <summary>
        /// Mentés parancsának lekérdezése.
        /// </summary>
        public RelayCommand SaveCommand { get; private set; }
        
        /// <summary>
        /// Alkalmazásból való kilépés eseménye.
        /// </summary>
        public event EventHandler? ExitApplication;

        /// <summary>
        /// Épület szerkesztés elindításának eseménye.
        /// </summary>
        public event EventHandler? BuildingEditingStarted;

        /// <summary>
        /// Épület szerkesztés befejeztének eseménye.
        /// </summary>
        public event EventHandler? BuildingEditingFinished;

        /// <summary>
        /// Képszerkesztés elindításának eseménye.
        /// </summary>
        public event EventHandler<BuildingEventArgs>? ImageEditingStarted;

        /// <summary>
        /// Nézetmodell példányosítása.
        /// </summary>
        /// <param name="model">A modell.</param>
        public MainViewModel(ITravelAgencyModel model)
        {
            _model = model;
            _model.BuildingChanged += Model_BuildingChanged;
            _isLoaded = false;

            CreateBuildingCommand = new RelayCommand(() =>
            {
                EditedBuilding = new BuildingDto() // a szerkesztett épület új lesz
                {
                    Name = string.Empty,
                    City = Cities.First(),
                    Comment = string.Empty,
                };
                OnBuildingEditingStarted();
            });
            UpdateBuildingCommand = new RelayCommand<BuildingDto>(UpdateBuilding);
            DeleteBuildingCommand = new RelayCommand<BuildingDto>(DeleteBuilding);
            CreateImageCommand = new RelayCommand<BuildingDto>(param =>
            {
                if (param != null)
                    OnImageEditingStarted(param.Id);
            });
            DeleteImageCommand = new RelayCommand<ImageDto>(DeleteImage);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            CancelChangesCommand = new RelayCommand(CancelChanges);
            LoadCommand = new RelayCommand(LoadAsync);
            SaveCommand = new RelayCommand(SaveAsync);
            ExitCommand = new RelayCommand( OnExitApplication);
        }

        /// <summary>
        /// Épület frissítése.
        /// </summary>
        /// <param name="building">Az épület.</param>
        private void UpdateBuilding(BuildingDto? building)
        {
            if (building == null)
                return;

            EditedBuilding = new BuildingDto
            {
                Id = building.Id,
                Name = building.Name,
                City = building.City,
                SeaDistance = building.SeaDistance,
                Shore = building.Shore,
                Features = building.Features.Select(feature => new FeatureDto
                {
                    Id = feature.Id,
                    IsAvailable = feature.IsAvailable
                }).ToArray(),
                LocationX = building.LocationX,
                LocationY = building.LocationY,
                Comment = building.Comment
            }; // a szerkesztett épület adatait áttöltjük a kijelöltből

            OnBuildingEditingStarted();
        }

        /// <summary>
        /// Épület törlése.
        /// </summary>
        /// <param name="building">Az épület.</param>
        private void DeleteBuilding(BuildingDto? building)
        {
            if (building == null || !Buildings.Contains(building))
                return;

            Buildings.Remove(building);

            _model.DeleteBuilding(building);
        }

        /// <summary>
        /// Kép törlése.
        /// </summary>
        /// <param name="image">A kép.</param>
        private void DeleteImage(ImageDto? image)
        {
            if (image == null)
                return;

            _model.DeleteImage(image);
        }

        /// <summary>
        /// Változtatások mentése.
        /// </summary>
        private void SaveChanges()
        {
            // ellenőrzések
            if (string.IsNullOrEmpty(EditedBuilding?.Name))
            {
                OnMessageApplication("Az épületnév nincs megadva!");
                return;
            }
            if (EditedBuilding?.City == null)
            {
                OnMessageApplication("A város nincs megadva!");
                return;
            }

            // mentés
            if (EditedBuilding.Id == 0) // ha új az épület
            {
                _model.CreateBuilding(EditedBuilding);
                Buildings.Add(EditedBuilding);
                SelectedBuilding = EditedBuilding;
            }
            else // ha már létezik az épület
            {
                _model.UpdateBuilding(EditedBuilding);
            }

            EditedBuilding = null;

            OnBuildingEditingFinished();
        }

        /// <summary>
        /// Változtatások elvetése.
        /// </summary>
        private void CancelChanges()
        {
            EditedBuilding = null;
            OnBuildingEditingFinished();
        }

        /// <summary>
        /// Betöltés végrehajtása.
        /// </summary>
        private async void LoadAsync()
        {
            try
            {
                await _model.LoadAsync();
                Buildings = new ObservableCollection<BuildingDto>(_model.Buildings); // az adatokat egy követett gyűjteménybe helyezzük
                Cities = new ObservableCollection<CityDto>(_model.Cities);
                IsLoaded = true;
            }
            catch (PersistenceUnavailableException)
            {
                OnMessageApplication("A betöltés sikertelen! Nincs kapcsolat a kiszolgálóval.");
            }
        }

        /// <summary>
        /// Mentés végreahajtása.
        /// </summary>
        private async void SaveAsync()
        {
            try
            {
                await _model.SaveAsync();
                OnMessageApplication("A mentés sikeres!");
            }
            catch (PersistenceUnavailableException)
            {
                OnMessageApplication("A mentés sikertelen! Nincs kapcsolat a kiszolgálóval.");
            }
        }

        /// <summary>
        /// Épület megváltozásának eseménykezelése.
        /// </summary>
        private void Model_BuildingChanged(object? sender, BuildingEventArgs e)
        {
            int index = Buildings.IndexOf(Buildings.First(building => building.Id == e.BuildingId));
            Buildings.RemoveAt(index); // módosítjuk a kollekciót
            Buildings.Insert(index, _model.Buildings[index]);

            SelectedBuilding = Buildings[index]; // és az aktuális épületet
        }
        
        /// <summary>
        /// Alkalmazásból való kilépés eseménykiváltása.
        /// </summary>
        private void OnExitApplication()
        {
            if (ExitApplication != null)
                ExitApplication(this, EventArgs.Empty);
        }

        /// <summary>
        /// Épület szerkesztés elindításának eseménykiváltása.
        /// </summary>
        private void OnBuildingEditingStarted()
        {
            if (BuildingEditingStarted != null)
                BuildingEditingStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Épület szerkesztés befejeztének eseménykiváltása.
        /// </summary>
        private void OnBuildingEditingFinished()
        {
            if (BuildingEditingFinished != null)
                BuildingEditingFinished(this, EventArgs.Empty);
        }

        /// <summary>
        /// Képszerkesztés elindításának eseménykiváltása.
        /// </summary>
        /// <param name="buildingId">Épület azonosító.</param>
        private void OnImageEditingStarted(int buildingId)
        {
            if (ImageEditingStarted != null)
                ImageEditingStarted(this, new BuildingEventArgs { BuildingId = buildingId });
        }
    }
}
