using Bomberman.Models.Database;

namespace Bomberman.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Adds user to the database.
        /// </summary>
        /// <param name="user"></param>
        void AddUser(User user);

        /// <summary>
        /// Encrypts given password by using the SHA-256 algorithm.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>The encrypted password</returns>
        string EncryptPassword(string password);

        /// <summary>
        /// Gets user by it's id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The user if it's present in the database,<br>Null otherwise.</br></returns>
        User? GetUserById(int id);

        /// <summary>
        /// Gets user by it's username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>The user if it's present in the database,<br>Null otherwise.</br></returns>
        User? GetUserByUsername(string username);

        /// <summary>
        /// Updates user's data and commits it to the database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        void UpdateUser(User user, string? username = null, string? password = null, string? email = null);

        /// <summary>
        /// Gets scores for the given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A list of the users scores.</returns>
        IEnumerable<Score> GetScoresForUser(User user);

        /// <summary>
        /// Adds score to the database for the given user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="score"></param>
        void AddScoreForUser(string username, Score score);

        /// <summary>
        /// Checks if there is a user record with the given email address.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if there is one,<br>False otherwise.</br></returns>
        public bool EmailIsInUse(string email);

        /// <summary>
        /// Registers a user.
        /// </summary>
        /// <exception cref="ArgumentException">If any of the user values are null.</exception>
        /// <param name="user"></param>
        /// <returns>The error message, if there were any problems with the input,<br>null otherwise.</br></returns>
        public string? TryRegister(User user);
    }
}