using Bomberman.Models.Characters;
using Bomberman.Models.Database;

namespace Bomberman.Models.Map
{
    public interface IGameModel
    {
        public event EventHandler<int> OnTick;
        public event EventHandler<EventArgs> OnGameEnd;
        public event EventHandler<(string, Score)>? OnPlayerDeath;

        /// <summary>
        /// Changes the player's state to ready.
        /// </summary>
        /// <param name="player">The player's name</param>
        public void Ready(string player);

        /// <summary>
        /// Changes the player's state to unready.
        /// </summary>
        /// <param name="player">The player's name.</param>
        public void UnReady(string player);

        /// <summary>
        /// Tries to move the player 1 step towards the given direction.
        /// </summary>
        /// <param name="player">The player's name.</param>
        /// <param name="direction">Moving direction.</param>
        public void Move(string player, Direction direction);

        /// <summary>
        /// Tries to place a bomb at the player's position.
        /// </summary>
        /// <param name="player">The player's name.</param>
        public void DropBomb(string player);

        /// <summary>
        /// Adds player to the lobby. Extra details:
        /// <br> - If <strong>isSpectator</strong> is true, user is added as a spectator, othewise the user is added as a player.</br>
        /// <br> - If the game is in progress the user is added as a spectator, regardless of the <strong>isSpectator</strong> parameter.</br>
        /// </summary>
        /// <param name="player">The player's name.</param>
        /// <param name="isSpectator">True if the player is a spectator, false otherwise.</param>
        public void Join(string player, bool isSpectator);

        /// <summary>
        /// Removes the player from the lobby. Extra details:
        /// <br> - If the game is in progress and the user is a player the statistics will show that he left this game.</br>
        /// <br> - If the game is in progress, but the user died it won't be shown in the statistics.</br>
        /// <br> - Otherwise, if the user is a spectator or the game hasn't started yet, the user will be removed from the lobby.</br>
        /// </summary>
        /// <param name="player">The player's name.</param>
        public void Leave(string player);

        /// <summary>
        /// There is a pair method named <strong>GetSpectatorNames</strong> for the spectator names.
        /// </summary>
        /// <returns>The players, which are still playing, those who aren't spectators (dead players count as spectators).</returns>
        public List<Player> GetPlayers();

        /// <summary>
        /// There is a pair method named <strong>GetPlayers</strong> for the player names.
        /// </summary>
        /// <returns>The players, which are spectators (dead players count as spectators).</returns>
        public List<string> GetSpectatorNames();

        /// <summary>
        /// Encodes the game state in a string, rules are shown in the implementation.
        /// </summary>
        /// <returns>The encoded string.</returns>
        public string EncodeGameStateToString();

        /// <returns><strong>True</strong> if the game is in progress, <strong>false</strong> otherwise.</returns>
        public bool IsGameInProgress();

        /// <returns>The elapsed time in <strong>miliseconds</strong> since the start of the game.</returns>
        public double TimeElapsed();

        /// <returns>Maximum capacity of this lobby.</returns>
        public int GetMaxPlayerCount();

        /// <returns>The name of the map used int this lobby.</returns>
        public string GetMapName();

        /// <returns>The identifier of this lobby.</returns>
        public int GetLobbyNum();

        /// <param name="player">The player's name.</param>
        /// <returns>Player's score if the player is in the game, -1 otherwise.</returns>
        public int GetPointsFor(string player);

        /// <summary>
        /// Returns statistics for players who have died.
        /// </summary>
        /// <returns>A list of player names and their corresponding scores.</returns>
        public List<(string player, Score score)> GetStats();
    }
}
