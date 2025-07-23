namespace ELTE.TravelAgency.Shared.Models
{
    public class ImageDto
    {
        /// <summary>
        /// Azonosító lekérdezése, vagy beállítása.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Épület azonosító lekérdezése, vagy beállítása.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Kis kép lekérdezése, vagy beállítása.
        /// </summary>
        public required byte[] ImageSmall { get; set; }

        /// <summary>
        /// Nagy kép lekérdezése, vagy beállítása.
        /// </summary>
        public required byte[] ImageLarge { get; set; }
    }
}
