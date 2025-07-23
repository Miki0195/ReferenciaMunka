namespace ELTE.TravelAgency.Web.Models
{
    /// <summary>
    /// Jellemző típusa.
    /// </summary>
    public class FeatureViewModel
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
    }
}