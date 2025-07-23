namespace Bomberman.Models.Items
{
    public class Fire : FloatingItem
    {
        //There are 6 stages:       (TimeLeft%)     (Code)
        //  0 - Just put down (0)   100% - 90%      Fire0
        //  1 - Middle stage (2)     90% - 80%      Fire2
        //  2 - Max stage (4)        80% - 60%      Fire4
        //  3 - 1 lower stage (3)    60% - 40%      Fire3
        //  4 - Middle stage (2)     40% - 20%      Fire2
        //  5 - 1 lower stage (1)    20% - 0%       Fire1
        public static double[] STAGES = new double[] { 0.9, 0.8, 0.6, 0.4, 0.2, 0 };
        public static double LIFETIME = 1000; //in miliseconds

        #region Variables

        private double _timeLeft;
        private double _delay;
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
                if (STAGES[CurrentStage] * LIFETIME >= _timeLeft)
                    CurrentStage += 1;
            }
        }
        public int CurrentStage
        {
            get { return _currentStage; }
            set { _currentStage = value; }
        }
        public bool IsVisible
        {
            get { return (LIFETIME - TimeLeft) > _delay; }
        }
        public string Player
        {
            get { return _player; }
            set { _player = value; }
        }

        #endregion

        #region Constructors

        public Fire((int x, int y) pos, int delay, string player) : base(pos)
        {
            _timeLeft = LIFETIME;
            _currentStage = 0;
            _delay = delay;
            _player = player;
        }

        #endregion
    }
}
