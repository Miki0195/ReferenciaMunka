using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ELTE.TravelAgency.DataAccess.Models
{
    public class Rent
    {
        public int Id { get; set; }
        public int ApartmentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [MaxLength(255)]
        public required string Name { get; set; }
        [MaxLength(255)]
        public required string Address { get; set; }
        [MaxLength(15)]
        public required string PhoneNumber { get; set; }
        [MaxLength(255)]
        public required string Email { get; set; }

        public Apartment Apartment { get; set; } = null!;

        /// <summary>
        /// Ütközik-e egy másik foglalással.
        /// </summary>
        /// <param name="startDate">A foglalás kezdete.</param>
        /// <param name="endDate">A foglalás vége.</param>
        /// <returns>Igaz, ha ütközik, egyébként hamis.</returns>
        public bool IsConflicting(DateTime startDate, DateTime endDate)
        {
            return StartDate.Date >= startDate.Date && StartDate.Date < endDate.Date ||
                   EndDate.Date > startDate.Date && EndDate.Date < endDate.Date ||
                   startDate.Date >= StartDate.Date && startDate.Date < EndDate.Date ||
                   endDate.Date > StartDate.Date && endDate.Date < EndDate.Date;
        }
    }
}
