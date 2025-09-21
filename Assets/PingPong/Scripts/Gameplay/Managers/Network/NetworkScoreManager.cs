using System;
using Gameplay.Ball;
using Gameplay.Game;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkScoreManager : BaseNetworkGameManager
    {
        public NetworkVariable<int> ScorePlayer1 { get; private set; } = new NetworkVariable<int>(0);
        public NetworkVariable<int> ScorePlayer2 { get; private set; } = new NetworkVariable<int>(0);

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
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ScorePlayer1.OnValueChanged += OnScoreChangedHandler;
            ScorePlayer2.OnValueChanged += OnScoreChangedHandler;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ScorePlayer1.OnValueChanged -= OnScoreChangedHandler;
            ScorePlayer2.OnValueChanged -= OnScoreChangedHandler;
        }

        private void AddScoreServer(byte playerId, int score)
        {
            Debug.Log("Adding score to player " + playerId);
            
            if (playerId == 1) ScorePlayer1.Value += score;
            if (playerId == 2) ScorePlayer2.Value += score;
            
            CheckWinConditions();
        }
        
        private void AddScoreServer(byte playerId) => AddScoreServer(playerId, 1);

        private void OnScoreChangedHandler(int oldValue, int newValue)
        {
            // Вызывается при изменении любого счёта, пробрасываем в UI
            OnScoreChanged?.Invoke(ScorePlayer1.Value, ScorePlayer2.Value, (byte)(newValue == ScorePlayer1.Value ? 1 : 2));
        }

        private void CheckWinConditions()
        {
            if (ScorePlayer1.Value >= ScoresToWin) GameEvents.OnGameEnd(1);
            if (ScorePlayer2.Value >= ScoresToWin) GameEvents.OnGameEnd(2);
        }

        public (int, int) GetScore() => (ScorePlayer1.Value, ScorePlayer2.Value);
    }
}