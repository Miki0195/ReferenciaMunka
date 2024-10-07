using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public class ShieldPowerUp : PowerUp
    {
        public ShieldPowerUp((int x, int y) pos) : base(pos) { }

        public override async void Use(Player player)
        {
            player.IsShielded = true;
            await Task.Delay(4000);
            player.IsShielded = false;
        }
        public override string ToString()
        {
            return "GOOD";
        }
    }
}
