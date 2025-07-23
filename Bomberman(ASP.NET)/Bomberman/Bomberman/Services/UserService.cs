using Bomberman.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Bomberman.Services
{
    public class UserService : IUserService
    {
        public readonly BombermanDbContext _dbContext;
        public UserService(BombermanDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public User? GetUserById(int id)
        {
            return _dbContext.Users.SingleOrDefault(u => u.Id == id);
        }
        public User? GetUserByUsername(string username)
        {
            return _dbContext.Users.SingleOrDefault(u => u.Username == username);
        }
        public string EncryptPassword(string password)
        {
            StringBuilder stringBuilder = new StringBuilder();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValue = sha256.ComputeHash(Encoding.ASCII.GetBytes(password));
                foreach (var i in hashValue)
                {
                    stringBuilder.Append($"{i:X2}");
                }
            }
            return stringBuilder.ToString();
        }
        public void UpdateUser(User user, string? username = null, string? password = null, string? email = null)
        {
            User? u = this.GetUserById(user.Id);
            if (u != null)
            {
                u = user;

                u.Username = username != null ? username : user.Username;
                u.Password = password != null ? this.EncryptPassword(password) : user.Password;
                u.Email = email != null ? email : user.Email;

                _dbContext.SaveChanges();
            }
        }
        public void AddUser(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
        public bool EmailIsInUse(string email)
        {
            return _dbContext.Users.Any(kk => kk.Email == email);
        }
        public string? TryRegister(User user)
        {
            try
            {
                if (user.Username == null
                || user.Password == null
                || user.Email == null)
                    return "All fields must be filled!";
            }
            catch (ArgumentException ex)
            {
                return $"{ex}";
            }
            
            if (user.Username.Contains(' '))
                return "Username shouldn't contain whitespace!";

            if (this.GetUserByUsername(user.Username) != null)
                return "Username is taken!";
            if (this.EmailIsInUse(user.Email))
                return $"'{user.Email}' is already in use!";

            user.Password = EncryptPassword(user.Password);
            this.AddUser(user);

            return null;
        }
        public IEnumerable<Score> GetScoresForUser(User user)
        {
            return _dbContext.Scores.Where(s => s.User == user);
        }
        public void AddScoreForUser(string username, Score score)
        {
            User? u = _dbContext.Users.SingleOrDefault(x => x.Username == username);

            if(u != null)
            {
                score.UserId = u.Id;
                _dbContext.Scores.Add(score);

                _dbContext.SaveChanges();
            }
        }
    }
}
