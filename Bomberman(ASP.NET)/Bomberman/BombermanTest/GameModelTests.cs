using Bomberman.Models.Characters;
using Bomberman.Models.Map;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.Identity.Client.Kerberos;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BombermanTest
{
    [TestClass]
    public class GameModelTests
    {
        private IGameModel _model = null!;
        private string mapPath = null!;
        public GameModelTests()
        {
            string[] paths = { ".", "map0.txt" };
            mapPath = Path.Combine(paths);
            using (StreamWriter f = new StreamWriter(mapPath))
            {
                f.WriteLine("1\t1\t1\t1\t1\t1");
                f.WriteLine("1\tX1\t1\tX2\t0\t1");
                f.WriteLine("1\t0\t1\t1\t0\t1");
                f.WriteLine("1\t0\t1\t0\t0\t1");
                f.WriteLine("1\t0\tX4\t2\tX3\t1");
                f.WriteLine("1\t1\t1\t1\t1\t1");
            }
        }
        ~GameModelTests()
        {
            File.Delete(mapPath);
        }

        [TestInitialize]
        public void Initialize()
        {
            _model = new GameModel(mapPath, 2, 0);
        }

        [TestMethod]
        public void JoinTest()
        {
            //Cant make path to map0.txt
            _model.Join("NewPlayer", false);

            var players = _model.GetPlayers();
            Assert.AreEqual(1, players.Count);
            Assert.AreEqual("NewPlayer", players[0].Name);

            _model.Join("NewPlayer", false);
            players = _model.GetPlayers();
            Assert.AreEqual(1, players.Count);
        }

        [TestMethod]
        public void LeaveTest()
        {
            //Cant make path to map0.txt
            _model.Join("NewPlayer", false);

            var players = _model.GetPlayers();
            Assert.AreEqual(1, players.Count);
            Assert.AreEqual("NewPlayer", players[0].Name);

            _model.Leave("NewPlayer");
            players = _model.GetPlayers();
            Assert.AreEqual(0, players.Count);
        }

        [TestMethod]
        public void ReadyTest()
        {
            _model.Join("NewPlayer", false);
            _model.Ready("NewPlayer");

            var players = _model.GetPlayers();
            Assert.IsTrue(players[0].IsReady);
        }

        [TestMethod]
        public void UnReadyTest()
        {
            _model.Join("NewPlayer", false);
            _model.Ready("NewPlayer");
            _model.UnReady("NewPlayer");

            var players = _model.GetPlayers();
            Assert.IsFalse(players[0].IsReady);
        }

        [TestMethod]
        public void IsGameInProgressTest()
        {
            Assert.IsFalse(_model.IsGameInProgress());

            _model.Join("NewPlayer", false);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer");

            Assert.IsFalse(_model.IsGameInProgress());

            _model.Ready("NewPlayer2");

            Assert.IsTrue(_model.IsGameInProgress());
        }

        [TestMethod]
        public void MoveTest()
        {
            _model.Join("NewPlayer", false);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer");
            _model.Ready("NewPlayer2");

            var players = _model.GetPlayers();

            int prevYCoordinate = players[0].Pos.y;

            _model.Move("NewPlayer", Direction.Down);

            Assert.AreEqual(prevYCoordinate + 1, players[0].Pos.y);
        }

        [TestMethod]
        public void DropBombTest()
        {
            _model.Join("NewPlayer", false);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer");
            _model.Ready("NewPlayer2");

            _model.DropBomb("NewPlayer");

            var players = _model.GetPlayers();
            string encoded = _model.EncodeGameStateToString();
            string floatingItemPart = encoded.Split(';')[3];
            Assert.IsTrue(floatingItemPart.Split('|').Any(kk => int.Parse(kk.Split(',')[0]) == players[0].Pos.y && int.Parse(kk.Split(',')[1]) == players[0].Pos.x && kk.Split(',')[2].Contains("Bomb")));
        }
        [TestMethod]
        public void GetSpectatorNamesTest()
        {
            _model.Join("NewPlayer", true);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer2");

            var spectators = _model.GetSpectatorNames();
            Assert.AreEqual(1, spectators.Count);
        }

        [TestMethod]
        public void TimeElapsedTest()
        {
            _model.Join("NewPlayer", false);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer");

            Assert.AreEqual(0, _model.TimeElapsed());
            
            _model.Ready("NewPlayer2");

            Thread.Sleep(100);

            Assert.IsTrue(0 < _model.TimeElapsed());
        }

        [TestMethod]
        public void GetMaxPlayerCountTest()
        {
            Assert.AreEqual(2, _model.GetMaxPlayerCount());
        }

        [TestMethod]
        public void GetMapNameTest()
        {
            Assert.AreEqual("map0", _model.GetMapName());
        }
        [TestMethod]
        public void GetLobbyNumTest()
        {
            Assert.AreEqual(0, _model.GetLobbyNum());
        }
        [TestMethod]
        public void GetPointsForTest()
        {
            _model.Join("NewPlayer", false);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer");
            _model.Ready("NewPlayer2");

            Assert.AreEqual(-1, _model.GetPointsFor("NewPlayer3"));
            Assert.AreEqual(0, _model.GetPointsFor("NewPlayer"));

            _model.DropBomb("NewPlayer");

            Assert.IsTrue(0 < _model.GetPointsFor("NewPlayer"));
        }

        [TestMethod]
        public void GetStatsTest()
        {
            _model.Join("NewPlayer", false);
            _model.Join("NewPlayer2", false);
            _model.Ready("NewPlayer");
            _model.Ready("NewPlayer2");
            _model.DropBomb("NewPlayer");

            Assert.AreEqual(0, _model.GetStats().Count);

            Thread.Sleep(1200);
            var stats = _model.GetStats();
            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("NewPlayer", stats[0].player);
        }
    }
}