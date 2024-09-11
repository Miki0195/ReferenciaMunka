using BombazoSajat.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;
using BombazoSajat.Persistence;
using System.Windows.Navigation;
using Microsoft.Windows.Themes;

namespace BeadandoSajatWPF.ViewModel
{
    public class BombazoViewModel : ViewModelBase
    {
        private BombazoGameModel _gameModel;

        private DispatcherTimer _timer;
        private int _gameTime;
        private ImageSource _playerPic = null!;
        private Thickness _playerMargin;
        private int _playerSize;
        private ImageSource _mapImage = null!;
        private Bitmap _bombBitmap;
        private bool _isNewGameEnabled;

        private Visibility _gameCanvasVisibility;

        public ObservableCollection<ViewModelEnemy> EnemyPics { get; set; }
        public ObservableCollection<ViewModelBomb> BombPics { get; set; }
        public DelegateCommand Map7x7Command { get; set; }
        public DelegateCommand Map10x10Command { get; set; }
        public DelegateCommand Map14x14Command { get; set; }
        public DelegateCommand NewGameCommand { get; set; }
        public DelegateCommand QuitGameCommand { get; set; }
        
        public int GameTime
        {
            get => _gameTime;
            set
            {
                _gameTime = value;
                OnPropertyChanged(nameof(GameTime));
                OnPropertyChanged(nameof(FormattedTime));
            }
        }
        public string FormattedTime
        {
            get
            {
                int hour = (GameTime / 3600);
                int minute = ((GameTime / 60) % 60);
                int seconds = (GameTime % 60);
                return $"{hour:D2}:{minute:D2}:{seconds:D2}";
            }
        }
        public string FormattedEnemyCountText => $"{_gameModel.EnemyCount}/{_gameModel.InnitialEnemyCount}";
        public ImageSource PlayerPic
        {
            get { return _playerPic; }
            set
            {
                _playerPic = value;
                OnPropertyChanged(nameof(PlayerPic));
            }
        }
        public Thickness PlayerMargin
        {
            get => _playerMargin;
            set
            {
                _playerMargin = value;
                OnPropertyChanged(nameof(PlayerMargin));
            }
        }
        public int PlayerSize
        {
            get => _playerSize;
            set
            {
                _playerSize = value;
                OnPropertyChanged(nameof(PlayerSize));
            }
        }
        public ImageSource MapImage
        {
            get { return _mapImage; }
            set
            {
                _mapImage = value;
                OnPropertyChanged(nameof(MapImage));
            }
        }
        public Visibility GameCanvasVisibility
        {
            get { return _gameCanvasVisibility; }
            set
            {
                _gameCanvasVisibility = value;
                OnPropertyChanged(nameof(GameCanvasVisibility));
            }
        }
        public bool IsNewGameEnabled
        {
            get { return _isNewGameEnabled; }
            set
            {
                _isNewGameEnabled = value;
                OnPropertyChanged(nameof(IsNewGameEnabled));
            }
        }

        public BombazoViewModel(BombazoGameModel gameModel)
        {
            _gameModel = gameModel;
            _gameModel.GameOverEvent += EndGame;
            _gameModel.MapLoadedEvent += DrawMap;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += _gameModel.TimerTick;
            _timer.Tick += MoveEnemys;
            _timer.Tick += DrawBombs;
            _timer.Tick += UpdateClockAndEnemyCount;
            _gameTime = 0;


            EnemyPics = new ObservableCollection<ViewModelEnemy>();
            MapImage = Converter(new Bitmap(1,1));

            GameCanvasVisibility = Visibility.Hidden;


            Map7x7Command = new DelegateCommand(param => ChooseSize("7x7"));
            Map10x10Command = new DelegateCommand(param => ChooseSize("10x10"));
            Map14x14Command = new DelegateCommand(param => ChooseSize("14x14"));
            NewGameCommand = new DelegateCommand(param => NewGame());
            QuitGameCommand = new DelegateCommand(param => QuitGame());

            BombPics = new ObservableCollection<ViewModelBomb>();
            _bombBitmap = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(_bombBitmap);
            g.FillEllipse(new SolidBrush(System.Drawing.Color.Purple), 30, 30, 40, 40);
        }
        
        private void UpdateClockAndEnemyCount(object? obj, EventArgs e)
        {
            OnPropertyChanged(nameof(_gameModel.EnemyCount));
            OnPropertyChanged(nameof(_gameModel.InnitialEnemyCount));
            OnPropertyChanged(nameof(FormattedEnemyCountText));

            GameTime++;
        }
        private void ChooseSize(string size)
        {
            IsNewGameEnabled = true;
            if (size == null)
            {
                return;
            }
            _gameModel?.SelectMap("DefaultMaps/map" + size + ".txt");

        }
        private void NewGame()
        {
            _gameModel?.StartGame();
            _timer.Start();
            GameCanvasVisibility = Visibility.Visible;
        }
        private void QuitGame() //Done
        {
            bool restartTimer = _timer.IsEnabled;
            _timer.Stop();
            if (MessageBox.Show("Biztosan ki szeretne lépni?", "Bombázó", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else
            {
                if (restartTimer)
                    _timer.Start();
            }
        }
        public void EndGame(object? obj, BombazoEventArgs e)
        {
            _timer.Stop();
            GameTime = 0;

            if (e.IsWon)
            {
                MessageBox.Show("Gratulálok, győztél!" + Environment.NewLine +
                                "Az összes ellenségedet megölted!",
                                "Bombázó játék",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
            else
            {
                MessageBox.Show("Sajnálom vesztettél!",
                                "Bombázó játék",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
            PlayerPic = Converter(new Bitmap(1,1));
            EnemyPics.Clear();
            BombPics.Clear();
            IsNewGameEnabled = false;
            GameCanvasVisibility = Visibility.Hidden;
            _gameModel.GameEnd();
        }

        private void DrawMap(object? obj, EventArgs e)
        {

            int tileSize = 630 / _gameModel._bombazoMap.Map.Length;
            _gameModel._bombazoMap.SetTileSize(tileSize);

            Bitmap _bitmap = new Bitmap(630, 630);
            Graphics g = Graphics.FromImage(_bitmap);

            System.Drawing.Brush blackBrush = new SolidBrush(System.Drawing.Color.Black);
            for (int i = 0; i < _gameModel._bombazoMap.Map.Length; i++)
            {
                for (int j = 0; j < _gameModel._bombazoMap.Map[i].Length; j++)
                {
                    if (_gameModel._bombazoMap.Map[i][j].IsRock())
                    {
                        g.FillRectangle(blackBrush, j * tileSize, i * tileSize, tileSize, tileSize);
                    }
                }
            }
            MapImage = Converter(_bitmap);

            //Player
            PlayerPic = Converter(new Bitmap(1,1));
            PlayerPic = Converter(_gameModel._bombazoMap.Player.Bitmap);
            PlayerSize = tileSize;
            PlayerMargin = new Thickness(_gameModel._bombazoMap.Player.Position.x * tileSize, _gameModel._bombazoMap.Player.Position.y * tileSize, 0, 0); //Player frissites


            //Enemy
            EnemyPics.Clear();

            foreach (var item in _gameModel._bombazoMap.Enemys)
            {
                EnemyPics.Add(new ViewModelEnemy(item, item.Bitmap, Converter(item.Bitmap), tileSize));
            }


        }
        public void MoveEnemys(object? obj, EventArgs e)
        {
            if (!_gameModel.InGame)
                return;

            for (int i = 0; i < EnemyPics.Count; i++)
            {
                if (_gameModel._bombazoMap.Enemys[i].ToBeDestroyed)
                {
                    _gameModel._bombazoMap.Enemys.RemoveAt(i);
                    EnemyPics.RemoveAt(i);
                    --i;
                    continue;
                }

                EnemyPics[i].RefreshImage();
            }
            CollectionViewSource.GetDefaultView(EnemyPics).Refresh();
        }
        public void KeyDownFunction(object? obj, KeyEventArgs e)
        {
            if (!_gameModel.InGame)
                return;

            int tileSize = 630 / _gameModel._bombazoMap.Map.Length;

            if (e.Key == Key.Escape)
            {
                if (_gameModel.Paused)
                {
                    _gameModel.ResumeGame();
                    _timer.Start();
                }
                else
                {
                    _gameModel.PauseGame();
                    _timer.Stop();
                }
            }
            //swtich
            switch (e.Key)
            {
                case Key.A:
                    _gameModel._bombazoMap.Player.Move('A');
                    break;
                case Key.S:
                    _gameModel._bombazoMap.Player.Move('S');
                    break;
                case Key.D:
                    _gameModel._bombazoMap.Player.Move('D');
                    break;
                case Key.W:
                    _gameModel._bombazoMap.Player.Move('W');
                    break;
                case Key.Space:
                    _gameModel._bombazoMap.Player.Move(' ');
                    break;
                default:
                    return;
            }
            if (!(_gameModel._bombazoMap.Player == null))
                PlayerMargin = new Thickness(_gameModel._bombazoMap.Player.Position.x * tileSize, _gameModel._bombazoMap.Player.Position.y * tileSize, 0, 0); //Player frissites



            if (e.Key == Key.Space)
            {
                DrawBombs(this, EventArgs.Empty);
            }
        }

        public void DrawBombs(object? obj, EventArgs e)
        {
            for (int i = 0; i < _gameModel._bombazoMap._bombs.Count; i++)
            {
                if (_gameModel._bombazoMap._bombs[i].Timer == 0)
                {
                    BombPics.RemoveAt(i);
                    _gameModel._bombazoMap._bombs.RemoveAt(i);
                    --i;
                }
                if (i >= BombPics.Count)
                {
                    BombPics.Add(new ViewModelBomb(_gameModel._bombazoMap._bombs[i], Converter(_bombBitmap), _gameModel._bombazoMap.TileSize));
                }
            }
        }

        #region Converter
        private BitmapSource Converter(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                 IntPtr.Zero,
                                 Int32Rect.Empty,
                                 BitmapSizeOptions.FromEmptyOptions());
        }
        #endregion

    }
}
