namespace ELTE.TravelAgency.DataAccess.Models
{
    public class BuildingImage
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public required byte[] ImageSmall { get; set; }
        public required byte[] ImageLarge { get; set; }

        public  Building Building { get; set; } = null!;
    }
}
