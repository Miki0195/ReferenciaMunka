using Bomberman.Models.Map;
using System.Numerics;

namespace BombermanTest
{
    [TestClass]
    public class GameModelTests
    {
        [TestMethod]
        public void JoinTest()
        {
            GameModel model = new GameModel("../Bomberman/Maps/map0.txt", 2);
            model.Join("NewPlayer");

            var players = model.GetPlayers();
            Assert.AreEqual(1, players.Count);
            Assert.AreEqual("NewPlayer", players[0].Name);

            model.Join("NewPlayer");
            Assert.AreEqual(1, players.Count);
        }
    }
}