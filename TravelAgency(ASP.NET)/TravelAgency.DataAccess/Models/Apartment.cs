using System;

namespace ELTE.TravelAgency.DataAccess.Models
{
    public class Apartment
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int Room { get; set; }

        public DayOfWeek Turnday { get; set; }
        public required string Comment { get; set; }
        public int Price { get; set; }

        public Building Building { get; set; } = null!;
    }
}