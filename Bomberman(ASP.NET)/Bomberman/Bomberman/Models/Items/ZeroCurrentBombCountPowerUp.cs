using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public class ZeroCurrentBombCountPowerUp : PowerUp
    {
        public ZeroCurrentBombCountPowerUp((int x, int y) pos) : base(pos) { }

        public override void Use(Player player)
        {
            player.CurrentBombCount = 0;
        }
        public override string ToString()
        {
            return "BAD";
        }
    }
}
