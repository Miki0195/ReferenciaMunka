using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public class MoreMaxBombsPowerUp : PowerUp
    {
        public MoreMaxBombsPowerUp((int x, int y) pos) : base(pos) { }

        public override void Use(Player player)
        {
            player.MaxBombCount += 1;
        }
        public override string ToString()
        {
            return "GOOD";
        }
    }
}
