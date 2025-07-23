using System;

namespace ELTE.TravelAgency.DataAccess.Models
{
    [Flags]
    public enum Feature
    {
        /// <summary>
        /// Semmi
        /// </summary>
        None = 0,
        /// <summary>
        /// Főút
        /// </summary>
        MainRoad = 1,
        /// <summary>
        /// Parti szolgálat
        /// </summary>
        CoastService = 2,
        /// <summary>
        /// Úszómedence
        /// </summary>
        SwimmingPool = 4,
        /// <summary>
        /// Kert
        /// </summary>
        Garden = 8,
        /// <summary>
        /// Saját parkolói
        /// </summary>
        PrivateParking = 16
    }
}