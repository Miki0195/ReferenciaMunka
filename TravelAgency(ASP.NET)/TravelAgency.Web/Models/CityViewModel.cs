using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.Web.Models
{
    public class CityViewModel
    {
        public int Id { get; set; }

        [StringLength(255)]
        public required string Name { get; set; }
    }
}
