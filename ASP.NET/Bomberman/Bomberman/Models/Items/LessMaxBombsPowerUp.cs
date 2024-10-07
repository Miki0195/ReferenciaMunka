using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public class LessMaxBombsPowerUp : PowerUp
    {
        public LessMaxBombsPowerUp((int x, int y) pos) : base(pos) { }

        public override void Use(Player player)
        {
            player.MaxBombCount = Math.Max(0, player.MaxBombCount);
        }
        public override string ToString()
        {
            return "BAD";
        }
    }
}
