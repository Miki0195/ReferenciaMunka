namespace ELTE.TravelAgency.DataAccess.Services
{
    /// <summary>
    /// Foglalási dátum hiba felsorolási típusa.
    /// </summary>
    public enum RentDateError
    {
        /// <summary>
        /// Nincs hiba.
        /// </summary>
        None,

        /// <summary>
        /// Hibás kezdődátum.
        /// </summary>
        StartInvalid,

        /// <summary>
        /// Hibás vége dátum.
        /// </summary>
        EndInvalid,

        /// <summary>
        /// Hibás hossz.
        /// </summary>
        LengthInvalid,

        /// <summary>
        /// Ütközés.
        /// </summary>
        Conflicting,

        /// <summary>
        /// Nem létező apartman.
        /// </summary>
        ApartmentNotExists
    }
}