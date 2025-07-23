using System;

namespace ELTE.TravelAgency.Web.Models
{
    public class ApartmentViewModel
    {
        public int Id { get; set; }

        public int Room { get; set; }

        public DayOfWeek Turnday { get; set; }

        public required string Comment { get; set; }

        public int Price { get; set; }

        public required BuildingViewModel Building { get; set; }
    }
}