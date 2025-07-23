namespace ELTE.TravelAgency.Shared.Models
{
    /// <summary>
    /// Jellemző típusa.
    /// </summary>
    public class FeatureDto
    {
        internal const int FeatureCount = 5;

        /// <summary>
        /// Jellemző azonosítója.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Jellemző elérhetősége.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Egyenlőségvizsgálat.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is FeatureDto dto &&
                   Id == dto.Id &&
                   IsAvailable == dto.IsAvailable;
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
