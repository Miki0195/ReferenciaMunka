using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public class PlusPointsPowerUp : PowerUp
    {
        public PlusPointsPowerUp((int x, int y) pos) : base(pos) { }

        public override void Use(Player player)
        {
            player.Score += 500;
        }
        public override string ToString()
        {
            return "GOOD";
        }
    }
}
