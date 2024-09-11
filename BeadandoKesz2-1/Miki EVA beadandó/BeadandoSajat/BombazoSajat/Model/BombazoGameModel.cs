using BombazoSajat.Persistence;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace BombazoSajat.Model
{
    public class BombazoGameModel
    {
        public int EnemyCount;
        public int InnitialEnemyCount;

        public event EventHandler<BombazoEventArgs>? GameOverEvent;
        public event EventHandler<EventArgs>? MapLoadedEvent;
        public BombazoMap _bombazoMap { get; private set; }
        public bool InGame { get; private set; }
        public bool Paused { get; private set; }
        public BombazoGameModel()
        {
            EnemyCount = 0;
            _bombazoMap = new BombazoMap(new BombazoFileAcces());
            Paused = true;
            InGame = false;
        }

        public void SelectMap(string map)
        {
            if(InGame)
                GameOverEvent?.Invoke(this,new BombazoEventArgs(false));
            _bombazoMap.Clear();
            _bombazoMap.LoadMap(map, this);
            InnitialEnemyCount = _bombazoMap.Enemys.Count;

            foreach (var item in _bombazoMap.Enemys)
                item.RandomDirection();

            MapLoadedEvent?.Invoke(this, EventArgs.Empty);
        }
        public void TimerTick(object? obj, EventArgs eventArgs)
        {
            foreach (var enemy in _bombazoMap.Enemys)
                enemy.EnemyMove();

            foreach (var bomb in _bombazoMap._bombs)
                --bomb.Timer;

            CheckGameStatus();

            EnemyCount = _bombazoMap.Enemys.Count;
            if (EnemyCount == 0 && InGame)
                GameOverEvent?.Invoke(this, new BombazoEventArgs(true));
        }
        public void CheckGameStatus()
        {
            for (int i = 0; i < _bombazoMap.Enemys.Count; i++)
            {
                Enemy enemy = _bombazoMap.Enemys[i];
                if (enemy.Position.x == _bombazoMap.Player.Position.x
                    && enemy.Position.y == _bombazoMap.Player.Position.y)
                {
                    Lose();
                    return;
                }

                foreach (var b in _bombazoMap._bombs.Where(kk => kk.Timer == 0))
                {
                    if (enemy.IsBlown(b))
                    {
                        enemy.ToBeDestroyed = true;
                        break;
                    }
				}
            }
            foreach (var b in _bombazoMap._bombs.Where(kk => kk.Timer == 0))
            {
                if (_bombazoMap.Player.IsBlown(b))
                {
                    Lose();
                    break;
                }
            }
        }
        public void Lose()
        {
            GameOverEvent?.Invoke(this, new BombazoEventArgs(false));
        }
        public void StartGame()
        {
            Paused = false;
            InGame = true;
        }
        public void PauseGame()
        {
            Paused = true;
        }
        public void ResumeGame()
        {
            Paused = false;
        }
        public void GameEnd()
        {
            _bombazoMap.Clear();
            InGame = false;
            Paused = true;
        }
    }
}