using System.Collections.Generic;
using System.Linq;

namespace ELTE.TravelAgency.Shared.Models
{
    /// <summary>
    /// Épület típusa.
    /// </summary>
    public class BuildingDto
    {
        /// <summary>
        /// Épület azonosítója.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Épület neve.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Város.
        /// </summary>
        public required CityDto City { get; set; }

        /// <summary>
        /// Tengerpart távolság.
        /// </summary>
        public int SeaDistance { get; set; }

        /// <summary>
        /// Tengerpart típusa.
        /// </summary>
        public ShoreTypeDto Shore { get; set; }

        /// <summary>
        /// Jellemzők.
        /// </summary>
        public FeatureDto[] Features { get; set; }

        /// <summary>
        /// X pozíció.
        /// </summary>
        public double LocationX { get; set; }

        /// <summary>
        /// Y pozíció.
        /// </summary>
        public double LocationY { get; set; }

        /// <summary>
        /// Megjegyzés.
        /// </summary>
        public required string Comment { get; set; }

        /// <summary>
        /// Képek.
        /// </summary>
        public IList<ImageDto> Images { get; set; }
        
        /// <summary>
        /// Apartmanok.
        /// </summary>
        public IList<ApartmentDto> Apartments { get; set; }

        /// <summary>
        /// Új <see cref="BuildingDto"/> objektum példányosítása.
        /// </summary>
        public BuildingDto()
        {
            Features = new FeatureDto[FeatureDto.FeatureCount];
            for (int i = 0; i < Features.Length; ++i)
            {
                Features[i] = new FeatureDto { Id = i };
            }

            Images = new List<ImageDto>();
            Apartments = new List<ApartmentDto>();
        }
        
        /// <summary>
        /// Egyenlőségvizsgálat.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is BuildingDto dto &&
                   Id == dto.Id &&
                   Name == dto.Name &&
                   City.Equals(dto.City) &&
                   SeaDistance == dto.SeaDistance &&
                   Shore == dto.Shore &&
                   Features.SequenceEqual(dto.Features) &&
                   LocationX == dto.LocationX &&
                   LocationY == dto.LocationY &&
                   Comment == dto.Comment;
        }

        /// <summary>
        /// Hash-kulcs generálás.
        /// </summary>
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
