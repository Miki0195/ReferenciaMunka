using Bomberman.Models.Characters;
using Bomberman.Models.Database;
using Bomberman.Models.Map;
using Bomberman.Services;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Linq.Expressions;
using static System.Formats.Asn1.AsnWriter;

namespace Bomberman.Models.SignalR
{
    public class Client
    {
        public string Name { get; set; } = null!;
        public bool IsSpectating { get; set; }
        public string ConnectionId { get; set; } = null!;
        public int LobbyNum { get; set; }
    }

    public class GameHub : Hub
    {
        #region Fields
        public static List<IGameModel> gameModel = new List<IGameModel>();
        public static List<Client> players = new List<Client>();
        private IUserService _userService;
        private IHubContext<GameHub> _hubContext;
        #endregion

        #region Constructors
        public GameHub(IUserService userService, IHubContext<GameHub> hubContext)
        {
            _userService = userService;
            _hubContext = hubContext;
        }
        #endregion

        #region Overrides
        public async override Task OnConnectedAsync()
        {
            var currPlayer = players.Single(kk => kk.Name == Context.User!.Identity!.Name!);

            int lobbyNum = currPlayer.LobbyNum;
            var currentGameModel = gameModel.Single(x => x.GetLobbyNum() == lobbyNum);
            currPlayer.ConnectionId = Context.ConnectionId;

            currentGameModel.Join(currPlayer.Name, currPlayer.IsSpectating);
            currentGameModel.OnTick += async (sender, args) =>
            {
                try
                {
                    await RefreshMap(sender, args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            };
            currentGameModel.OnPlayerDeath += async (sender, args) =>
            {
                if (currPlayer.Name != args.Item1)
                    return;
                
                try
                {
                    await ReciveFinalScore(sender, args.Item2, currPlayer.Name, currPlayer.ConnectionId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            };
            currentGameModel.OnGameEnd += async (sender, args) =>
            {
                try
                {
                    await ReciveGameEnd(currentGameModel.GetLobbyNum());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            };

            await NewJoin(lobbyNum);

            await base.OnConnectedAsync();
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            int lobbyNum = GetLobbyNumForUsername(Context.User!.Identity!.Name!);

            var currentGameModel = gameModel.SingleOrDefault(x => x.GetLobbyNum() == lobbyNum);
            if (currentGameModel != null)
            {
                currentGameModel.Leave(Context.User!.Identity!.Name!);

                if (currentGameModel.GetPlayers().Count == 0)
                {
                    foreach (var s in currentGameModel.GetStats())
                    {
                        if (s.score.Points > 0) _userService.AddScoreForUser(s.player, s.score);
                    }

                    gameModel.Remove(currentGameModel);
                }
            }

            players.Remove(players.Single(x => x.ConnectionId == Context.ConnectionId));

            //So that the player list on the right refreshes
            var currentGameModel2 = gameModel.Single(x => x.GetLobbyNum() == lobbyNum);
            await ForClientsIn(lobbyNum).SendAsync("OnNewJoin", players.Where(x => x.LobbyNum == lobbyNum).Select(x => x.Name), currentGameModel2.GetPlayers().Count(x => x.IsReady), currentGameModel2.GetMaxPlayerCount());

            //So that the map refreshes for the other players
            await RefreshMap(this, lobbyNum);

            await base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region Public (Handler) Methods
        /// <summary>
        /// Sends the recived input to clients (used for project demo #2)
        /// </summary>
        /// <param name="input">The character recived</param>
        public async Task SendInput(char input)
        {
            int lobbyNum = GetLobbyNumForUsername(Context.User!.Identity!.Name!);

            var currentGameModel = gameModel.Single(x => x.GetLobbyNum() == lobbyNum);

            if (currentGameModel.IsGameInProgress())
            {
                switch (char.ToLower(input))
                {
                    case 'w':
                        currentGameModel.Move(Context.User!.Identity!.Name!, Direction.Up);
                        break;

                    case 'a':
                        currentGameModel.Move(Context.User!.Identity!.Name!, Direction.Left);
                        break;

                    case 's':
                        currentGameModel.Move(Context.User!.Identity!.Name!, Direction.Down);
                        break;

                    case 'd':
                        currentGameModel.Move(Context.User!.Identity!.Name!, Direction.Right);
                        break;

                    case ' ':
                        currentGameModel.DropBomb(Context.User!.Identity!.Name!);
                        break;

                }
            }
            else
                await ForClientsIn(lobbyNum).SendAsync("ReceiveInput", Context.User!.Identity!.Name, input);
        }

        /// <summary>
        /// Updates and sends the game state to clients
        /// </summary>
        public async Task RefreshMap(object? sender, int lobbyNum)
        {
            var currentGameModel = gameModel.SingleOrDefault(x => x.GetLobbyNum() == lobbyNum);

            if (currentGameModel != null)
                await ForClientsIn(lobbyNum).SendAsync("OnGameUpdate", currentGameModel.EncodeGameStateToString(), currentGameModel.IsGameInProgress());
        }

        /// <summary>
        /// Adds a new player to the player list
        /// </summary>
        /// <param name="lobbyNum">The lobby of the caller client</param>
        public async Task NewJoin(int lobbyNum)
        {
            var currentGameModel = gameModel.Single(x => x.GetLobbyNum() == lobbyNum);
            await ForClientsIn(lobbyNum).SendAsync("OnNewJoin", players.Where(x => x.LobbyNum == lobbyNum).Select(x => x.Name), currentGameModel.GetPlayers().Count(x => x.IsReady), currentGameModel.GetMaxPlayerCount());
            await RefreshMap(this, lobbyNum);
        }

        /// <summary>
        /// Sets 'Ready' for the player client who called
        /// </summary>
        public async Task ReceiveReady(bool isReady)
        {
            int lobbyNum = GetLobbyNumForUsername(Context.User!.Identity!.Name!);

            var currentGameModel = gameModel.Single(x => x.GetLobbyNum() == lobbyNum);

            if(isReady)
                currentGameModel.Ready(Context.User!.Identity!.Name!);
            else
                currentGameModel.UnReady(Context.User!.Identity!.Name!);

            await ForClientsIn(lobbyNum).SendAsync("OnReady", currentGameModel.GetPlayers().Count(x => x.IsReady), currentGameModel.GetMaxPlayerCount());

            await RefreshMap(this, lobbyNum);
        }

        /// <summary>
        /// Sends the caller's final score to the correct lobby
        /// </summary>
        public async Task ReciveFinalScore(object? sender, Score score, string username, string connectionId)
        {
            await ForClient(connectionId).SendAsync("OnDeath", username, score.Points.ToString());
        }

        public async Task ReciveGameEnd(int lobbyNum)
        {
            await ForClientsIn(lobbyNum).SendAsync("OnGameEnd");
        }

        #endregion

        #region Private (Helper) Methods
        /// <summary>
        /// Gets the clients lobby number
        /// </summary>
        /// <param name="username">Username :)</param>
        private int GetLobbyNumForUsername(string username)
        {
            return players.Single(x => x.Name == username).LobbyNum;
        }

        /// <summary>
        /// Gets a client list to send messages to
        /// </summary>
        /// <param name="lobbyNum">Lobby number to send to</param>
        /// <returns>IClientProxy to send messages to</returns>
        private IClientProxy ForClientsIn(int lobbyNum)
        {
            IReadOnlyList<string> selectedClients = players
                .Where(x => x.LobbyNum == lobbyNum)
                .Select(x => x.ConnectionId).ToList();

            return _hubContext.Clients.Clients(selectedClients);
        }

        private IClientProxy ForClient(string connectionId)
        {
            return _hubContext.Clients.Client(connectionId);
        }
        #endregion
    }
}
