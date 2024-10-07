using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public class InvertedControlsPowerUp : PowerUp
    {
        public InvertedControlsPowerUp((int x, int y) pos) : base(pos) { }

        public override async void Use(Player player)
        {
            player.AreControlsInverted = true;
            await Task.Delay(5000);
            player.AreControlsInverted = false;
        }
        public override string ToString()
        {
            return "BAD";
        }
    }
}
