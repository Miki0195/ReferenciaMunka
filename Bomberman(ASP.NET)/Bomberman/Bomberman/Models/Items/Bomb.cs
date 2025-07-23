namespace Bomberman.Models.Items
{
    public class Bomb : FloatingItem
    {
        //There are 3 stages:         (TimeLeft%)
        //  0 - Just put down (small) 100% - 66%
        //  1 - Middle stage (medium)  66% - 33%
        //  2 - About to blow (Large)  33% - 0%
        public static double[] STAGES = new double[] { 2/3.0, 1/3.0 };
        public static double LIFETIME = 1000; //in miliseconds

        #region Variables

        private double _timeLeft;
        private int _currentStage;
        private string _player;

        #endregion

        #region Properties

        public double TimeLeft
        {
            get { return _timeLeft; }
            set
            {
                _timeLeft = value;
                if (CurrentStage < STAGES.Length && STAGES[CurrentStage] * LIFETIME >= _timeLeft)
                    CurrentStage += 1;
            }
        }
        public int CurrentStage
        {
            get { return _currentStage; }
            set { _currentStage = value; }
        }
        public string Player
        {
            get { return _player; }
            set { _player = value; }
        }

        #endregion

        #region Constructors

        public Bomb((int x, int y) pos, string player) : base(pos)
        {
            _timeLeft = LIFETIME;
            _currentStage = 0;
            _player = player;
        }

        #endregion
    }
}
