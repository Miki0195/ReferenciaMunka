using Bomberman.Models.Map;
using Bomberman.Models.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberman.Controllers
{
    public class GameController : Controller
    {
        private static int NexId = 0;

        [Authorize]
        public IActionResult Games()
        {
            List<LobbyDisplayViewModel> model = new List<LobbyDisplayViewModel>();

            foreach (var i in GameHub.gameModel)
            {
                var players = i.GetPlayers();
                if (players.Count == 1 && players[0].Name == HttpContext.User.Identity!.Name!)
                    continue;

                model.Add(new LobbyDisplayViewModel
                {
                    Map = i.GetMapName(),
                    MaxPlayers = i.GetMaxPlayerCount(),
                    CurrentPlayers = i.GetPlayers().Count(),
                    LobbyNum = i.GetLobbyNum()
                });
            }

            return View(model);
        }

        [Authorize]
        public IActionResult Create()
        {
            LobbyCreateViewModel model = new LobbyCreateViewModel
            {
                Maps = GetMaps()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(LobbyCreateViewModel model)
        {
            int lobbyNum = NexId++;
            if (ModelState.IsValid)
            {
                if (GameHub.players.Any(x => x.LobbyNum == lobbyNum))
                    throw new ArgumentException("Another lobby already has the same id!");

                string[] paths = { ".", "Maps", $"{model.Map}.txt" };
                string mapPath = Path.Combine(paths);

                IGameModel gameModel = new GameModel(mapPath, model.MaxPlayers, lobbyNum);

                GameHub.gameModel.Add(gameModel);
                return RedirectToAction("Go", new { lobbyNum = lobbyNum });
            }

            model.Maps = GetMaps();
            return View(model);
        }

        [Authorize]
        public IActionResult Go(int lobbyNum, bool isSpectating)
        {
            if (GameHub.gameModel.All(kk => kk.GetLobbyNum() != lobbyNum))
            {
                TempData["PopupTextForGames"] = "This lobby has been terminated since.";
                return RedirectToAction("Games");
            }
            var game = GameHub.gameModel.FirstOrDefault(kk => kk.GetLobbyNum() == lobbyNum);
            if (!isSpectating && game != null)
            {
                if (game.IsGameInProgress())
                {
                    TempData["PopupTextForGames"] = "This game has already begun. You may only join as a spectator.";
                    return RedirectToAction("Games");
                }
                if (game.GetPlayers().Count == game.GetMaxPlayerCount())
                {
                    TempData["PopupTextForGames"] = "This lobby is full. You may only join as a spectator.";
                    return RedirectToAction("Games");
                }
            }


            Client client = new Client
            {
                Name = HttpContext.User.Identity!.Name!,
                LobbyNum = lobbyNum,
                IsSpectating = isSpectating,
                ConnectionId = ""
            };

            GameHub.players.Add(client);

            return RedirectToAction("Play");
        }

        [Authorize]
        public IActionResult Play()
        {
            if (GameHub.players.Any(x => x.Name == HttpContext.User.Identity!.Name))
                return View();

            else return RedirectToAction("Games");
        }

        private List<string> GetMaps()
        {
            string[] paths = { ".", "Maps" };
            string mapsDirectory = Path.Combine(paths);
            string[] mapFiles = Directory.GetFiles(mapsDirectory);

            return mapFiles.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
        }
    }
}
