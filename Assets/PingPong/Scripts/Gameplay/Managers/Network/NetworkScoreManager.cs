using System;
using Gameplay.Ball;
using Gameplay.Game;
using Unity.Netcode;

namespace Gameplay.Managers
{
    public class NetworkScoreManager : BaseNetworkGameManager
    {
        public int ScorePlayer1 { get; private set; }
        public int ScorePlayer2 { get; private set; }

        private const int ScoresToWin = 11;

        public static event Action<int, int, byte> OnScoreChanged;

        protected override void SubscribeEvents()
        {
            if (IsServer)
                BallEvents.OnBallScored += AddScoreServer;
        }

        protected override void UnsubscribeEvents()
        {
            if (IsServer)
                BallEvents.OnBallScored -= AddScoreServer;
        }

        private void AddScoreServer(byte playerId, int score)
        {
            if (playerId == 1) ScorePlayer1 += score;
            if (playerId == 2) ScorePlayer2 += score;

            // Синхронизируем счёт с клиентами
            UpdateScoreClientRpc(ScorePlayer1, ScorePlayer2, playerId);

            CheckWinConditions();
        }
        
        private void AddScoreServer(byte playerId) => AddScoreServer(playerId, 1);

        [ClientRpc]
        private void UpdateScoreClientRpc(int score1, int score2, byte playerId)
        {
            OnScoreChanged?.Invoke(score1, score2, playerId);
        }

        private void CheckWinConditions()
        {
            if (ScorePlayer1 >= ScoresToWin) GameEvents.OnGameEnd(1);
            if (ScorePlayer2 >= ScoresToWin) GameEvents.OnGameEnd(2);
        }

        public (int, int) GetScore() => (ScorePlayer1, ScorePlayer2);
    }
}