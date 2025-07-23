namespace ELTE.Cinema.Web.Models
{
    public class MovieViewModel
    {
        /// <summary>
        /// Unique identifier for the movie
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Title of the movie
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Year of release
        /// </summary>
        public int Year { get; init; }

        /// <summary>
        /// Director of the movie
        /// </summary>
        public required string Director { get; init; }

        /// <summary>
        /// Short synopsis of the movie
        /// </summary>
        public required string Synopsis { get; init; }

        /// <summary>
        /// Length of the movie in minutes
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// Image or poster for the movie
        /// </summary>
        public required byte[] Image { get; init; }
    }
}
