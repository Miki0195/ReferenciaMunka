namespace Bomberman.Models.Items
{
    public abstract class FloatingItem
    {
        #region Variables

        private (int x, int y) _pos;

        #endregion

        #region Properties

        public (int x, int y) Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        #endregion

        #region Constructors

        public FloatingItem()
        {
            _pos = (0, 0);
        }
        public FloatingItem((int x, int y) pos)
        {
            _pos = pos;
        }

        #endregion
    }
}
