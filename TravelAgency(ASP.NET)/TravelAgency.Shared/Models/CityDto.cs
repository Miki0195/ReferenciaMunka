namespace ELTE.TravelAgency.Shared.Models
{
    /// <summary>
    /// Város típusa.
    /// </summary>
    public class CityDto
    {
        /// <summary>
        /// Város azonosítója.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Város neve.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Egyenlőségvizsgálat.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is CityDto dto &&
                   Id == dto.Id &&
                   Name == dto.Name;
        }

        /// <summary>
        /// Hash-kulcs generálás.
        /// </summary>
        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Szöveggé alakítás.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
