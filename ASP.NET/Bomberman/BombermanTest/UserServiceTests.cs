using Bomberman.Models.Database;
using Bomberman.Models.Map;
using Bomberman.Services;
using Microsoft.EntityFrameworkCore;

namespace BombermanTest
{
    [TestClass]
    public class UserServiceTests : IDisposable
    {
        private BombermanDbContext _context = null!;
        private IUserService _userService = null!;
        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<BombermanDbContext>().UseInMemoryDatabase("TestDb").Options;

            _context = new BombermanDbContext(options);
            _userService = new UserService(_context);
            DbInitializer.Initialize(_context, _userService);

            _context.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void IfAUsernameIsNotPresentInDatabase_TheServiceReturnsNull()
        {
            User? user = _userService.GetUserByUsername("notpresent");
            Assert.IsNull(user);
        }

        [TestMethod]
        public void IfAUsernameIsPresentInDatabase_TheServiceReturnsTheCorrectUser()
        {
            User? user = _userService.GetUserByUsername("test2");
            Assert.IsNotNull(user);

            Assert.AreEqual("test2", user.Username);
            Assert.AreEqual(_userService.EncryptPassword("test"), user.Password);
            Assert.AreEqual("test2@testdomain.com", user.Email);
        }

        [TestMethod]
        [DataRow(2, "test2")]
        public void IfAnIdIsPresentInDatabase_TheServiceReturnsTheCorrectUser(int id, string username)
        {
            User? user = _userService.GetUserById(id);
            Assert.IsNotNull(user);

            Assert.AreEqual(username, user.Username);
        }

        [TestMethod]
        public void IfAnIdIsNotPresentInDatabase_TheServiceReturnsNull()
        {
            int id = 69;
            
            User? user = _userService.GetUserById(id);
            Assert.IsNull(user);
        }

        [TestMethod]
        [DataRow(1, "testUpdated", "newPassword", "updatedtest@testdomain.com")]
        public void IfAUserThatIsValidIsUpdatedUsingParams_TheDataChangesForTheUser(int id, string username, string password, string email)
        {
            User? user = _userService.GetUserById(id);
            Assert.IsNotNull(user);

            _userService.UpdateUser(user, username, password, email);

            Assert.AreEqual(user.Username, username);
            Assert.AreEqual(user.Password, _userService.EncryptPassword(password));
            Assert.AreEqual(user.Email, email);
        }
    }
}