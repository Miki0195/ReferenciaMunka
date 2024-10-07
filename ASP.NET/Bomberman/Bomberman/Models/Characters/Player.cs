using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Bomberman.Models.Characters
{
    public class Player : Character
    {
        private const int BOMB_RECHARGE_TIME = 1000; //In milliseconds

        #region Variables

        private string _name;
        private int _maxBombCount;
        private int _currentBombCount;
        private int _score;
        private bool _isReady;
        private double _rechargeTime;
        private bool _areControlsInverted;
        private bool _isShielded;

        #endregion

        #region Properties

        public bool IsShielded
        {
            get { return _isShielded; }
            set { _isShielded = value; }
        }
        public bool IsReady
        {
            get { return _isReady; }
            set { _isReady = value; }
        }
        public bool AreControlsInverted
        {
            get { return _areControlsInverted; }
            set { _areControlsInverted = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public int MaxBombCount
        {
            get { return _maxBombCount; }
            set { _maxBombCount = value; }
        }
        public int CurrentBombCount
        {
            get { return _currentBombCount; }
            set { _currentBombCount = value; }
        }
        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }
        public double RechargeTime
        {
            get { return _rechargeTime; }
            set
            {
                if (CurrentBombCount < MaxBombCount)
                {
                    _rechargeTime = value;
                    if (_rechargeTime <= 0)
                    {
                        _rechargeTime = BOMB_RECHARGE_TIME;
                        CurrentBombCount += 1;
                    }
                }
            }
        }

        #endregion

        #region Constructors

        public Player(string name) : base()
        {
            _name = name;
            _maxBombCount = 1;
            _currentBombCount = 1;
            _score = 0;
            _rechargeTime = BOMB_RECHARGE_TIME;
        }
        public Player(string name, (int x, int y) pos, Direction direction) : base(pos, direction)
        {
            _name = name;
            _maxBombCount = 1;
            _currentBombCount = 1;
            _score = 0;
            _rechargeTime = BOMB_RECHARGE_TIME;
        }

        #endregion
    }
}
