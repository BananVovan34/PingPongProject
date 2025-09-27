using System;
using PingPong.Scripts.Gameplay.Ball;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkScoreManager : NetworkBehaviour
    {
        public static NetworkScoreManager Instance { get; private set; }
        
        public NetworkVariable<int> player1Points = new NetworkVariable<int>();
        public NetworkVariable<int> player2Points = new NetworkVariable<int>();

        private const int PointsToWin = 11;
        
        public event Action<byte> OnScoredPointsToWin;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            ResetScores();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            BallController ball = FindFirstObjectByType(typeof(BallController)) as BallController;

            if (ball != null)
                ball.OnScoreZoneReached += HandleScoreZoneReached;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            BallController ball = FindFirstObjectByType(typeof(BallController)) as BallController;
            
            if (ball != null)
                ball.OnScoreZoneReached -= HandleScoreZoneReached;
        }

        private void HandleScoreZoneReached(string zoneTag)
        {
            if (zoneTag == "ScoreZoneLeft")
                player2Points.Value++;
            else if (zoneTag == "ScoreZoneRight")
                player1Points.Value++;
            
            Debug.Log(player1Points.Value + " : " + player2Points.Value);

            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            if (player1Points.Value == PointsToWin) OnScoredPointsToWin?.Invoke(0);
            if (player2Points.Value == PointsToWin) OnScoredPointsToWin?.Invoke(1);
        }

        private void ResetScores()
        {
            player1Points.Value = 0;
            player2Points.Value = 0;
        }
    }
}