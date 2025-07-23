using Bomberman.Models.Characters;

namespace Bomberman.Models.Items
{
    public abstract class PowerUp : FloatingItem
    {
        #region Constructors

        protected PowerUp((int x, int y) pos) : base(pos)
        {

        }

        #endregion

        public virtual async void Use(Player player) { await Task.Delay(1); }
    }
}
