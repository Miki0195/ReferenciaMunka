using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BombazoSajat.Model;

namespace BombazoSajat.Persistence
{
    public class BombazoMap
    {
        #region public variables

        public int TileSize { get; private set; }
        public void SetTileSize(int value)
        {
            if (value > 0)
                TileSize = value;
        }
        public BombazoPlatform[][] Map { get; private set; } //allits értéket
        public List<Enemy> Enemys { get; private set; }
        public List<Bomb> _bombs { get; private set; }
        public Player Player;
        #endregion

        #region private variables
        private IBombazoDataAcces _dataAcces;
        #endregion

        public BombazoMap(IBombazoDataAcces dataAcces)
        {
            TileSize = 50;
            Player = null!;
            Enemys = new List<Enemy>();
            _bombs = new List<Bomb>();
            Map = null!;
            _dataAcces = dataAcces;
        }
        public void Clear()
        {
            TileSize = 50;
            Player = null!;
            Map = null!;
            Enemys.Clear();
            _bombs.Clear();
        }

        public void LoadMap(string path, BombazoGameModel model)
        {
            var data = _dataAcces.Load(path);

            Map = new BombazoPlatform[data.Length][];

            for (int i = 0; i < data.Length; i++)
            {
                var temp = data[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                Map[i] = new BombazoPlatform[temp.Length];
                for (int j = 0; j < temp.Length; j++)
                    switch (temp[j].Trim())
                    {
                        case "0":
                            Map[i][j] = new Floor();
                            break;
                        case "1":
                            Map[i][j] = new Rock();
                            break;
                        case "2": //Enemy
                            Map[i][j] = new Floor();
                            Bitmap enemyBitmap = new Bitmap(100, 100);
                            Graphics enemyBitmapG = Graphics.FromImage(enemyBitmap);
                            enemyBitmapG.FillPolygon(new SolidBrush(Color.Red), new Point[] { new Point(50, 0), new Point(0, 100), new Point(100, 100) });
                            Enemys.Add(new Enemy((j, i), model, enemyBitmap));
                            break;
                        default:
                            throw new ArgumentException("Nem megfelelő karakter a pálya fájljában.");
                    }
            }

            Bitmap playerBitmap = new Bitmap(100,100);
            Graphics g = Graphics.FromImage(playerBitmap);
            g.FillEllipse(new SolidBrush(Color.Blue), 25, 25, 50, 50);

            Player = new Player((0, 0), model, playerBitmap);
        }

    }
}
