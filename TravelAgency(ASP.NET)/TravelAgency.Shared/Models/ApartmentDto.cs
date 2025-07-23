using System;

namespace ELTE.TravelAgency.Shared.Models;

public class ApartmentDto
{
        /// <summary>
        /// Apartman azonosítója.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Épület azonosítója.
        /// </summary>
        public int BuildingId { get; set; }
        
        /// <summary>
        /// Szobák száma.
        /// </summary>
        public int Room { get; set; }
        
        /// <summary>
        /// Váltás napja.
        /// </summary>
        public DayOfWeek Turnday { get; set; }
        
        /// <summary>
        /// Megjegyzés.
        /// </summary>
        public required string Comment { get; set; }
        
        /// <summary>
        /// Ár.
        /// </summary>
        public int Price { get; set; }
}