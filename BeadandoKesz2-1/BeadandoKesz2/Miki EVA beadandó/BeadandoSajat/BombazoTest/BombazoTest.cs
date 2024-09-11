using BombazoSajat;
using BombazoSajat.Model;
using BombazoSajat.Persistence;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.DataCollection;
using Moq;
using System.Drawing;

namespace BombazoTest
{
    [TestClass]
    public class BombazoTest
    {
        private BombazoGameModel _gameModel = null!;

        [TestInitialize]
        public void Initialize()
        {
            _gameModel = new BombazoGameModel();

        }
        [TestMethod]
        public void ConstructorTest()
        {
            Assert.AreEqual(0, _gameModel.EnemyCount);
            Assert.IsNotNull(_gameModel._bombazoMap);
            Assert.IsTrue(_gameModel.Paused);
            Assert.IsFalse(_gameModel.InGame);
        }
        [TestMethod]
        public void SelectMap_WithCorrectMap_LoadsMap()
        {
            _gameModel.SelectMap("map7x7.txt");
            Assert.IsNotNull(_gameModel._bombazoMap.Map);
            _gameModel.SelectMap("map10x10.txt");
            Assert.IsNotNull(_gameModel._bombazoMap.Map);
            _gameModel.SelectMap("map14x14.txt");
            Assert.IsNotNull(_gameModel._bombazoMap.Map);
        }
        [TestMethod]
        public void SelectMap_WithNoMap_GeneratesErrorTest()
        {
            Assert.ThrowsException<Exception>(() => _gameModel.SelectMap(null!));
        }

        [TestMethod]
        public void SelectMap_WithIncorrectMap_GeneratesErrorTest()
        {
            string incorrectMap = "invalidMap";

            Assert.ThrowsException<Exception>(() => _gameModel.SelectMap(incorrectMap));
        }
        [TestMethod]
        public void TimerTickTest()
        {
            _gameModel.SelectMap("blankmap.txt");
            _gameModel._bombazoMap.Enemys.Add(new Enemy((5, 5), _gameModel, null!));
            _gameModel._bombazoMap.Enemys.Add(new Enemy((4, 5), _gameModel, null!));
            List<(int x, int y)> initialEnemyPositions = _gameModel._bombazoMap.Enemys.Select(e => e.Position).ToList();

            _gameModel.TimerTick(null, EventArgs.Empty);

            List<(int x, int y)> finalEnemyPositions = _gameModel._bombazoMap.Enemys.Select(e => e.Position).ToList();
            Assert.AreNotEqual(initialEnemyPositions, finalEnemyPositions);
        }
        [TestMethod]
        public void CheckGameStatus()
        {
            _gameModel.SelectMap("blankmap.txt");
            _gameModel._bombazoMap.Enemys.Add(new Enemy((5,5), _gameModel, null!));
            _gameModel._bombazoMap.Player.Position = (5,5); 

            bool GameOverEventTriggered = false;
            _gameModel.GameOverEvent += (sender, args) =>
            {
                if (args.IsWon == false) 
                {
                    GameOverEventTriggered = true;
                }
            };

            _gameModel.CheckGameStatus();

            Assert.IsTrue(GameOverEventTriggered);
        }
        [TestMethod]
        public void StartGameTest()
        {
            _gameModel.StartGame();

            Assert.IsFalse(_gameModel.Paused);
            Assert.IsTrue(_gameModel.InGame);
        }
        [TestMethod]
        public void PauseGameTest()
        {
            _gameModel.StartGame();
            _gameModel.PauseGame();
            Assert.IsTrue(_gameModel.Paused);
            _gameModel.StartGame();
            Assert.IsFalse(_gameModel.Paused);
        }
        [TestMethod]
        public void ResumeGameTest()
        {
            _gameModel.ResumeGame();
            Assert.IsFalse(_gameModel.Paused);
        }
        [TestMethod]
        public void GameEndTest()
        {
            _gameModel.StartGame();
            _gameModel.GameEnd();
            Assert.IsFalse(_gameModel.InGame);
            Assert.IsTrue(_gameModel.Paused);
        }
        [TestMethod]
        public void EventsGetInvokedTest()
        {
            bool MapLoadedEvenetTriggered = false;
            _gameModel.MapLoadedEvent += (sender, args) => MapLoadedEvenetTriggered = true;
            _gameModel.SelectMap("map7x7.txt");
            Assert.IsTrue(MapLoadedEvenetTriggered);

            bool GameOverEventTriggered = false;
            _gameModel.GameOverEvent += (sender, args) => GameOverEventTriggered = true;
            _gameModel.Lose();
            Assert.IsTrue(GameOverEventTriggered);
        }


    }
}