using Bomberman.Models.Characters;
using Bomberman.Models.Database;
using Bomberman.Models.Items;
using System.Text;
using Timer = System.Timers.Timer;

namespace Bomberman.Models.Map
{
    public class GameModel : IGameModel
    {
        #region Constants
        private const int TIME_UNTIL_ZONE_STARTS = 30000; //In milliseconds
        private const int ZONE_MOVE_TIME = 10000; //In milliseconds

        private const int TIME_BEFORE_GAME_END = 1000; //In milliseconds
        private const int FIRE_SPREAD_TIME = 200; //In milliseconds
        private const int FPS = 60; //Frames per second

        private const int MONSTER_MOVE_TIME = 750; //In milliseconds
        private const int MIN_MONSTER_RANDOM_DIRECTION_TIME = 500; //In milliseconds
        private const int MAX_MONSTER_RANDOM_DIRECTION_TIME = 2000; //In milliseconds

        #endregion

        #region Variables

        public event EventHandler<int>? OnTick;
        public event EventHandler<EventArgs>? OnGameEnd;
        public event EventHandler<(string, Score)>? OnPlayerDeath;

        private readonly Timer _timer;

        //Pos(x,y) => map[y][x]
        private int maxPlayerCount;
        private Field[][] map;
        private List<(int x, int y)> playerPositions;
        private Dictionary<string, Player?> players; //Null means spectating
        private List<Monster> monsters;
        private List<FloatingItem> floatingItems;
        private Dictionary<string, int> points;
        private List<(string player, Score score)> stats;

        private double timeElapsed;
        private double timeLeftToEnd;
        private readonly int lobbyNum;
        private readonly string mapName;

        private readonly Random random;

        private double zoneTime;
        private int zoneLength;

        #endregion

        #region Constructor

        public GameModel(string mapPath, int maxPlayerCount, int lobbyNum)
        {
            OnTick = null;
            OnGameEnd = null;
            random = new Random();
            points = new Dictionary<string, int>();

            if (mapPath.Contains('\\'))
                mapName = mapPath.Split('\\')[^1].Split('.')[0];
            else
                mapName = mapPath.Split('/')[^1].Split('.')[0];

            this.lobbyNum = lobbyNum;

            _timer = new Timer();
            _timer.Interval = 1000.0 / FPS;
            _timer.Elapsed += TimerTick;
            players = new Dictionary<string, Player?>();
            if (maxPlayerCount < 2 || maxPlayerCount > 4)
                throw new ArgumentException("Max player count must be in the range [2,4].");
            this.maxPlayerCount = maxPlayerCount;

            monsters = new List<Monster>();
            stats = new List<(string player, Score score)>();

            floatingItems = new List<FloatingItem>();

            timeLeftToEnd = TIME_BEFORE_GAME_END;
            timeElapsed = 0;

            this.lobbyNum = lobbyNum;

            playerPositions = new List<(int x, int y)>();
            map = LoadMap(mapPath);

            zoneTime = 0;
            zoneLength = 0;

            notNullPlayers = new List<KeyValuePair<string, Player?>>();
            killedPlayers = new HashSet<string>();
        }

        #endregion

        #region Public Methods

        public void Ready(string player)
        {
            if (!players.ContainsKey(player) || players[player] == null || IsGameInProgress())
                return;

            players[player]!.IsReady = true;

            List<Player> nonSpectatingPlayers = players.Where(kk => kk.Value != null).Select(kk => kk.Value).ToList()!;
            if (nonSpectatingPlayers.Count == maxPlayerCount && nonSpectatingPlayers.All(kk => kk.IsReady))
                StartGame();
        }
        public void UnReady(string player)
        {
            if (!players.ContainsKey(player) || players[player] == null || IsGameInProgress())
                return;

            players[player]!.IsReady = false;
        }
        public void Move(string player, Direction direction)
        {
            if (!players.ContainsKey(player) || players[player] == null)
                return;

            if (players[player]!.AreControlsInverted)
                switch (direction)
                {
                    case Direction.Up:
                        direction = Direction.Down;
                        break;
                    case Direction.Down:
                        direction = Direction.Up;
                        break;
                    case Direction.Left:
                        direction = Direction.Right;
                        break;
                    default: //Direction.Right:
                        direction = Direction.Left;
                        break;
                }

            (int x, int y) pos = players[player]!.Pos;
            (int x, int y) newPos;
            switch (direction)
            {
                case Direction.Up:
                    newPos = (pos.x, pos.y - 1);
                    break;
                case Direction.Down:
                    newPos = (pos.x, pos.y + 1);
                    break;
                case Direction.Left:
                    newPos = (pos.x - 1, pos.y);
                    break;
                default: //Direction.Right:
                    newPos = (pos.x + 1, pos.y);
                    break;
            }

            //Change direction even if the player us unable to move, so that we can show that the input really happened
            players[player]!.Direction = direction;

            if (newPos.x < 0 || newPos.y < 0 || newPos.y >= map.Length || newPos.x >= map[newPos.y].Length
                || floatingItems.Any(kk => kk.Pos == newPos && kk is Bomb)
                || map[newPos.y][newPos.x] != Field.Empty)
                return;

            players[player]!.Pos = newPos;
        }
        public void DropBomb(string player)
        {
            if (!IsGameInProgress() || !players.ContainsKey(player) || players[player] == null)
                return;

            Player p = players[player]!;
            if (p.CurrentBombCount == 0)
                return;

            points[player] += 10;
            p.CurrentBombCount--;
            floatingItems.Add(new Bomb(p.Pos, player));
        }
        public void Join(string player, bool isSpectator)
        {
            if (players.ContainsKey(player))
                return;

            if (isSpectator || IsGameInProgress() || players.Count(kk => kk.Value != null) == maxPlayerCount) //Even if the game is in progress, spectators can still join
            {
                players.Add(player, null);
                return;
            }

            points.Add(player, 0);
            players.Add(player, new Player(player, playerPositions[players.Count(kk => kk.Value != null)], Direction.Down));
        }
        public void Leave(string player)
        {
            if (IsGameInProgress() && players[player] != null) //Game is running and he didn't die
            {
                int placementValue = players.Count(kk => kk.Value != null);
                stats.Add((player, new Score
                {
                    Points = points[player],
                    Date = DateTime.Now,
                    Context = $"TimeElapsed:{timeElapsed};Placement:{placementValue};MaxPlayerCount:{maxPlayerCount};Won:{false};Left:{true}"
                }));
            }
            players.Remove(player);
        }
        public List<Player> GetPlayers()
        {
            return players.Values.Where(kk => kk != null).ToList()!;
        }
        public List<string> GetSpectatorNames()
        {
            return players.Where(kk => kk.Value == null).Select(kk => kk.Key).ToList();
        }
        public string EncodeGameStateToString()
        {
            // Format: All in a single line, now seperated for clarity's sake
            // <MaxPlayerCount>;<Player>,<PlayerScore>,<PlayerCurrentBombCount>,<PlayerMaxBombCount><PlayerPosY><PlayerPosX><PlayerFacing>...(repeats);
            // <[0][0] Tile : 0-Empty, 1-Wall, 2-Box><[0][1]>...<[0][n - 1]>,<[1][0]]>..<[n - 1][n - 1]>;
            // ;<posx>,<posy>,<itemAsString_withExtraInfoForItem>|... (repeats)
            // Rows are seperated with ',' when describing the map
            // Items are seperated with '|' when describing the floating items list
            // Floating items list contains monsters

            // 2;test,10,test2,20;3;3;000,000,000;2;1,0,Bomb1|1,2,Bomb3

            List<Direction> directions = new List<Direction>(4) { Direction.Up, Direction.Right, Direction.Down, Direction.Left }; //.IndexOf(character.Direction)

            StringBuilder sb = new StringBuilder();
            sb.Append(maxPlayerCount);
            sb.Append(';');
            sb.Append(string.Join(",", this.GetPlayers().Select(kk => $"{kk.Name},{points[kk.Name]},{kk.CurrentBombCount},{kk.MaxBombCount},{kk.Pos.y},{kk.Pos.x},{directions.IndexOf(kk.Direction)},{(kk.IsShielded ? "1" : "0")}")));
            sb.Append(';');
            foreach (var row in map)
            {
                foreach (var item in row)
                    switch (item)
                    {
                        case Field.Empty:
                            sb.Append('0');
                            break;
                        case Field.Wall:
                            sb.Append('1');
                            break;
                        case Field.Box:
                            sb.Append('2');
                            break;
                    }
                sb.Append(',');
            }
            sb.Length -= 1;

            sb.Append(';');

            foreach (var item in monsters)
                sb.Append($"{item.Pos.y},{item.Pos.x},Monster{directions.IndexOf(item.Direction)}|");

            foreach (var item in floatingItems.Where(kk => kk is Bomb))
            {
                Bomb asBomb = (Bomb)item;
                sb.Append($"{item.Pos.y},{item.Pos.x},Bomb{asBomb.CurrentStage}|");
            }
            foreach (var item in floatingItems.Where(kk => kk is PowerUp))
                sb.Append($"{item.Pos.y},{item.Pos.x},{item.ToString()}|");
            //Fire
            List<Fire> fireList = floatingItems.Where(kk => kk is Fire && ((Fire)kk).IsVisible).Select(kk => (Fire)kk).ToList();
            foreach (var item in fireList)
            {
                sb.Append($"{item.Pos.y},{item.Pos.x}");
                if (item.CurrentStage == 0) //only one with stage 0
                {
                    sb.Append($",FireM0|");
                    continue;
                }
                bool top = fireList.Any(kk => kk.Pos == (item.Pos.x, item.Pos.y - 1) && kk.CurrentStage == item.CurrentStage);
                bool bottom = fireList.Any(kk => kk.Pos == (item.Pos.x, item.Pos.y + 1) && kk.CurrentStage == item.CurrentStage);
                bool left = fireList.Any(kk => kk.Pos == (item.Pos.x - 1, item.Pos.y) && kk.CurrentStage == item.CurrentStage);
                bool right = fireList.Any(kk => kk.Pos == (item.Pos.x + 1, item.Pos.y) && kk.CurrentStage == item.CurrentStage);

                //Top ^
                if (bottom && !top && !left && !right)
                    sb.Append(",FireT");
                //Bottom v
                else if (!bottom && top && !left && !right)
                    sb.Append(",FireB");
                //Left <
                else if (!bottom && !top && left && !right)
                    sb.Append(",FireL");
                //Right >
                else if (!bottom && !top && !left && right)
                    sb.Append(",FireR");
                //Vertical |
                else if (bottom && top && !left && !right)
                    sb.Append(",FireV");
                //Horizontal -
                else if (!bottom && !top && left && right)
                    sb.Append(",FireH");
                //Middle .
                else
                    sb.Append(",FireM");

                sb.Append($"{item.CurrentStage}|");
            }
            if (floatingItems.Count > 0 || monsters.Count > 0)
                sb.Length -= 1;

            sb.Append(';');
            sb.Append(zoneLength);
            sb.Append(';');
            sb.Append(string.Join(",", this.GetSpectatorNames()));

            return sb.ToString();
        }
        public bool IsGameInProgress()
        {
            return _timer.Enabled;
        }
        public double TimeElapsed()
        {
            return timeElapsed;
        }
        public int GetMaxPlayerCount()
        {
            return maxPlayerCount;
        }
        public string GetMapName()
        {
            return mapName;
        }
        public int GetLobbyNum()
        {
            return lobbyNum;
        }
        public int GetPointsFor(string player)
        {
            if (!points.ContainsKey(player))
                return -1;
            return points[player];
        }
        public List<(string player, Score score)> GetStats()
        {
            return stats;
        }

        #endregion

        #region Private Methods

        //Variables across Timer Tick methods:
        private List<KeyValuePair<string, Player?>> notNullPlayers;
        private HashSet<string> killedPlayers;
        private void TimerTick(object? sender, EventArgs e)
        {
            notNullPlayers = players.Where(kk => kk.Value != null).ToList();
            killedPlayers = new HashSet<string>();

            //TimeElapsed
            timeElapsed += _timer.Interval;

            //Zone
            ZoneGameTick();

            //Bombs
            BombGameTick();

            //Fire
            FireGameTick();

            //Monsters
            MonsterGameTick();

            //Shield
            killedPlayers.RemoveWhere(kk => players[kk]!.IsShielded);

            //Player stats and removal
            PlayerGameTick();

            //PowerUps
            PowerUpGameTick();

            //Win conditions
            WinGameTick();

            OnTick?.Invoke(this, lobbyNum);
        }
        private void ZoneGameTick()
        {
            //Zone timer and growth
            if (TIME_UNTIL_ZONE_STARTS < timeElapsed)
                zoneTime += _timer.Interval;
            if (zoneTime > ZONE_MOVE_TIME)
            {
                zoneLength += 1;
                zoneTime %= ZONE_MOVE_TIME;
            }
            //Zone kills
            foreach (var item in notNullPlayers)
            {
                if (item.Value!.Pos.x < zoneLength ||
                    map[0].Length - item.Value.Pos.x - 1 < zoneLength ||
                    item.Value.Pos.y < zoneLength ||
                    map.Length - item.Value.Pos.y - 1 < zoneLength)
                    killedPlayers.Add(item.Key);
            }
        }
        private void BombGameTick()
        {
            //Bomb recharging
            foreach (var item in notNullPlayers)
                item.Value!.RechargeTime -= _timer.Interval;

            //Bombs
            foreach (var item in floatingItems.Where(kk => kk is Bomb).ToArray()) //.ToArray() is needed so that the original iterator can be changed
            {
                var asBomb = (Bomb)item;
                asBomb.TimeLeft -= _timer.Interval;
                if (asBomb.TimeLeft <= 0)
                {
                    floatingItems.Remove(item);
                    BlowBomb(asBomb);
                }
            }
        }
        private void FireGameTick()
        {
            var visibleFire = floatingItems.Where(kk => kk is Fire && ((Fire)kk).IsVisible).ToArray();//.ToArray() is needed so that the original iterator can be changed
            foreach (var item in visibleFire)
            {
                string playerResponsible = ((Fire)item).Player;
                //monsters
                var killedMonsters = monsters.Where(kk => kk.Pos == item.Pos).ToList(); //ToList is needed so the foreach can modify the original iterator
                points[playerResponsible] += 100 * killedMonsters.Count;
                foreach (var monster in killedMonsters)
                    monsters.Remove(monster);

                //players
                var currKilled = notNullPlayers.Where(kk => kk.Value!.Pos == item.Pos).Select(kk => kk.Key);
                points[playerResponsible] += 500 * currKilled.Count(kk => kk != playerResponsible);
                foreach (var i in currKilled)
                    killedPlayers.Add(i);

                //box
                if (map[item.Pos.y][item.Pos.x] == Field.Box)
                {
                    points[playerResponsible] += 50;
                    map[item.Pos.y][item.Pos.x] = Field.Empty;
                    switch (random.Next(0, 6))
                    {
                        case 0:
                            floatingItems.Add(new MoreMaxBombsPowerUp(item.Pos));
                            break;
                        case 1:
                            floatingItems.Add(new LessMaxBombsPowerUp(item.Pos));
                            break;
                        case 2:
                            floatingItems.Add(new InvertedControlsPowerUp(item.Pos));
                            break;
                        case 3:
                            floatingItems.Add(new ShieldPowerUp(item.Pos));
                            break;
                        case 4:
                            floatingItems.Add(new ZeroCurrentBombCountPowerUp(item.Pos));
                            break;
                        default:
                            floatingItems.Add(new PlusPointsPowerUp(item.Pos));
                            break;
                    }
                }
            }
            //Fire time calculations
            foreach (var item in floatingItems.Where(kk => kk is Fire).ToArray()) //.ToArray() is needed so that the original iterator can be changed
            {
                var asFire = (Fire)item;
                asFire.TimeLeft -= _timer.Interval;
                if (asFire.TimeLeft <= 0)
                    floatingItems.Remove(item);
            }
        }
        private void MonsterGameTick()
        {
            foreach (var item in monsters)
            {
                MoveMonster(item);
                foreach (var i in notNullPlayers.Where(kk => kk.Value!.Pos == item.Pos).Select(kk => kk.Key))
                    killedPlayers.Add(i);
            }
        }
        private void PlayerGameTick()
        {
            int alivePlayerCount = notNullPlayers.Count();
            bool didWin = false;
            if (alivePlayerCount == killedPlayers.Count) //In case of ties
                didWin = true;
            foreach (var item in killedPlayers)
            {
                var score = new Score
                {
                    Points = points[item],
                    Date = DateTime.Now,
                    Context = $"TimeElapsed:{timeElapsed};Placement:{alivePlayerCount};MaxPlayerCount:{maxPlayerCount};Won:{didWin};Left:{false}"
                };
                stats.Add((item, score));
                players[item] = null;

                OnPlayerDeath?.Invoke(this,(item, score));
            }
        }
        private void PowerUpGameTick()
        {
            foreach (var item in floatingItems.Where(kk => kk is PowerUp).ToArray())//.ToArray() is needed so that the original iterator can be changed
            {
                bool used = false;
                foreach (var playerToBeAppliedTo in players.Where(kk => kk.Value != null && kk.Value!.Pos == item.Pos).Select(kk => kk.Value))
                {
                    ((PowerUp)item).Use(playerToBeAppliedTo!);
                    used = true;
                }

                if (used)
                    floatingItems.Remove(item);
            }
        }
        private void WinGameTick()
        {
            int alivePlayerCount = notNullPlayers.Count();
            if (alivePlayerCount - killedPlayers.Count == 0)
            {
                if (timeLeftToEnd <= 0)
                    EndGame();
                timeLeftToEnd -= _timer.Interval;
            }
            else if (alivePlayerCount - killedPlayers.Count == 1)
            {
                if (timeLeftToEnd <= 0)
                {
                    string player = players.First(kk => kk.Value != null).Key;
                    stats.Add((player, new Score
                    {
                        Points = points[player],
                        Date = DateTime.Now,
                        Context = $"TimeElapsed:{timeElapsed};Placement:{1};MaxPlayerCount:{maxPlayerCount};Won:{true};Left:{false}"
                    }));

                    EndGame();
                }
                timeLeftToEnd -= _timer.Interval;
            }
        }

        private void BlowBomb(Bomb bomb)
        {
            (int x, int y) pos = bomb.Pos;
            //Pattern: +
            //Middle
            floatingItems.Add(new Fire((pos.x, pos.y), 0, bomb.Player));
            //Right:
            for (int i = pos.x + 1; i <= Math.Min(pos.x + 2, map[pos.y].Length - 1); i++)
            {
                if (map[pos.y][i] == Field.Wall)
                    break;

                floatingItems.Add(new Fire((i, pos.y), FIRE_SPREAD_TIME * Math.Abs(pos.x - i), bomb.Player));

                if (map[pos.y][i] == Field.Box) //Box will be shattered by the fire when a tick happens
                    break;
            }
            //Left
            for (int i = pos.x - 1; i >= Math.Max(0, pos.x - 2); i--)
            {
                if (map[pos.y][i] == Field.Wall)
                    break;

                floatingItems.Add(new Fire((i, pos.y), FIRE_SPREAD_TIME * Math.Abs(pos.x - i), bomb.Player));

                if (map[pos.y][i] == Field.Box) //Box will be shattered by the fire when a tick happens
                    break;
            }
            //Up
            for (int i = pos.y - 1; i >= Math.Max(0, pos.y - 2); i--)
            {
                if (map[i][pos.x] == Field.Wall)
                    break;

                floatingItems.Add(new Fire((pos.x, i), FIRE_SPREAD_TIME * Math.Abs(pos.y - i), bomb.Player));

                if (map[i][pos.x] == Field.Box) //Box will be shattered by the fire when a tick happens
                    break;
            }
            //Down
            for (int i = pos.y + 1; i <= Math.Min(pos.y + 2, map.Length - 1); i++)
            {
                if (map[i][pos.x] == Field.Wall)
                    break;

                floatingItems.Add(new Fire((pos.x, i), FIRE_SPREAD_TIME * Math.Abs(pos.y - i), bomb.Player));

                if (map[i][pos.x] == Field.Box) //Box will be shattered by the fire when a tick happens
                    break;
            }
        }
        private void StartGame()
        {
            _timer.Start();
        }
        private void EndGame()
        {
            _timer.Stop();
            timeElapsed = 0;
            OnGameEnd?.Invoke(this, EventArgs.Empty);
        }
        public void MoveMonster(Monster monster)
        {
            if (monster.MoveTimer > 0)
            {
                monster.MoveTimer -= _timer.Interval;
                return;
            }
            monster.MoveTimer = MONSTER_MOVE_TIME;

            (int x, int y) pos = monster.Pos;
            (int x, int y) newPos;
            switch (monster.Direction)
            {
                case Direction.Up:
                    newPos = (pos.x, pos.y - 1);
                    break;
                case Direction.Down:
                    newPos = (pos.x, pos.y + 1);
                    break;
                case Direction.Left:
                    newPos = (pos.x - 1, pos.y);
                    break;
                default: //Direction.Right:
                    newPos = (pos.x + 1, pos.y);
                    break;
            }

            //Something in the way, doesn't move, just turns randomly
            if (newPos.x < 0 || newPos.y < 0 || newPos.y >= map.Length || newPos.x >= map[newPos.y].Length
                || floatingItems.Any(kk => kk.Pos == newPos && kk is Bomb)
                || map[newPos.y][newPos.x] != Field.Empty
                || monster.RandomDirectionChangeTime <= 0) //If the random timer runs out
            {
                monster.Direction = RandomizeDirectionWithout(monster.Direction); //Get a random direction except current
                MoveMonster(monster);
                return;
            }
            else
                monster.Pos = newPos;

            //Random direction change
            if (monster.RandomDirectionChangeTime > 0)
                monster.RandomDirectionChangeTime -= _timer.Interval;
            else
                monster.RandomDirectionChangeTime = random.Next(MIN_MONSTER_RANDOM_DIRECTION_TIME, MAX_MONSTER_RANDOM_DIRECTION_TIME);
        }
        private Direction RandomizeDirectionWithout(Direction? direction = null)
        {
            var directions = new List<Direction> { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
            if (direction != null)
                directions.Remove((Direction)direction);
            return directions[random.Next(0, 3)];
        }
        private Field[][] LoadMap(string path)
        {
            string[] all = File.ReadAllLines(path);

            Field[][] solution = new Field[all.Length][];

            PriorityQueue<(int x, int y), int> prq = new PriorityQueue<(int x, int y), int>();
            for (int i = 0; i < all.Length; i++)
            {
                var splitted = all[i].Split('\t');
                solution[i] = new Field[splitted.Length];
                for (int j = 0; j < splitted.Length; j++)
                {
                    if (splitted[j][0] == 'X')
                    {
                        prq.Enqueue((j, i), splitted[j][1] - '0');
                        solution[i][j] = Field.Empty;
                        continue;
                    }

                    switch (splitted[j])
                    {
                        case "O":
                            monsters.Add(new Monster(random.Next(MIN_MONSTER_RANDOM_DIRECTION_TIME, MAX_MONSTER_RANDOM_DIRECTION_TIME), (j, i), RandomizeDirectionWithout(null)));
                            solution[i][j] = Field.Empty;
                            break;
                        case "0":
                            solution[i][j] = Field.Empty;
                            break;
                        case "1":
                            solution[i][j] = Field.Wall;
                            break;
                        case "2":
                            solution[i][j] = Field.Box;
                            break;
                        default:
                            break;
                    }
                }
            }

            while (prq.Count > 0)
                playerPositions.Add(prq.Dequeue());

            return solution;
        }

        #endregion
    }
}
