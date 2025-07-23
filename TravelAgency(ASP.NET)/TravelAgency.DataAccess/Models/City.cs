using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.DataAccess.Models
{
    public class City
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public required string Name { get; set; }

        public ICollection<Building> Buildings { get; set; } = [];
    }
}
