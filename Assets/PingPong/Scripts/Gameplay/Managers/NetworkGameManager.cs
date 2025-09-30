using System;
using System.Collections;
using System.Linq;
using PingPong.Scripts.Gameplay.Ball;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkGameManager : NetworkBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }
        
        private GameState _currentGameState;

        private int _lastCountOfConnectedClients;
        
        public event Action OnGameStart;
        public event Action<byte> OnGameEnd;
        public event Action OnRoundStart;
        public event Action OnRoundEnd;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _currentGameState = GameState.WaitingForPlayers;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkPlayerManager.Instance.OnClientConnected += TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected += ClientDisconnected;
            
            NetworkScoreManager.Instance.OnScoredPointsToWin += EndGame;

            BallController.Instance.OnScoreZoneReached += EndRound;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            NetworkPlayerManager.Instance.OnClientConnected -= TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected -= ClientDisconnected;
            
            NetworkScoreManager.Instance.OnScoredPointsToWin -= EndGame;
            
            BallController.Instance.OnScoreZoneReached -= EndRound;
        }

        private void ClientDisconnected(ulong obj)
        {
            int countOfConnectedClients = NetworkPlayerManager.Instance.ConnectedClients.Count;
            
            if (countOfConnectedClients <= 1 && _currentGameState == GameState.Playing)
            {
                var playerIds = NetworkPlayerManager.Instance.PlayerIds;
                byte winnerId = playerIds.Contains((byte) 0) ? (byte) 0 : (byte) 1;
                
                EndGame(winnerId);
            }
        }

        private void TryStartGame(ulong obj)
        {
            Debug.Log("TryStartGame");
            int countOfConnectedClients = NetworkPlayerManager.Instance.ConnectedClients.Count;
            
            if (countOfConnectedClients >= 2 && _currentGameState == GameState.WaitingForPlayers)
                StartGame();
        }

        private void StartGame()
        {
            Debug.Log("StartGame");
            _currentGameState = GameState.Playing;
            OnGameStart?.Invoke();
            StartRound();
        }

        private void EndRound(string obj)
        {
            OnRoundEnd?.Invoke();
            StartCoroutine(BeforeStartRound());
        }

        private IEnumerator BeforeStartRound()
        {
            yield return new WaitForSeconds(2.5f);
            StartRound();
        }

        private void StartRound()
        {
            OnRoundStart?.Invoke();
        }

        private void EndGame(byte winnerId)
        {
            _currentGameState = GameState.End;
            OnGameEnd?.Invoke(winnerId);
        }
    }

    internal enum GameState
    {
        WaitingForPlayers,
        Playing,
        End
    }
}