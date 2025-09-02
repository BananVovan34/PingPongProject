using System;
using Gameplay.Ball;

namespace Gameplay.Managers
{
    public class ScoreManager : BaseManager
    {
        public int ScorePlayer1 { get; private set; }
        public int ScorePlayer2 { get; private set; }

        private const int ScoresToWin = 11;
    
        public static event Action<int, int, byte> OnScoreChanged;

        private void Start()
        {
            ScorePlayer1 = 0;
            ScorePlayer2 = 0;
        }

        protected override void SubscribeEvents()
        {
            BallEvents.OnBallScored += AddScore;
        }

        protected override void UnsubscribeEvents()
        {
            BallEvents.OnBallScored -= AddScore;
        }

        public void AddScore(byte playerId, int score)
        {
            if (playerId == 1) ScorePlayer1 += score;
            if (playerId == 2) ScorePlayer2 += score;
        
            OnScoreChanged?.Invoke(ScorePlayer1, ScorePlayer2, playerId);
        }

        public void AddScore(byte playerId) { AddScore(playerId, 1); }
    
        public (int, int) GetScore() => (ScorePlayer1, ScorePlayer2);
    }
}
