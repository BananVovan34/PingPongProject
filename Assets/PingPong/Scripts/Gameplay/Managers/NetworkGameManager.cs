using System;
using System.Collections;
using PingPong.Scripts.Gameplay.Ball;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkGameManager : NetworkBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }
        
        private GameState _currentGameState;

        private int _lastCountOfConnectedClients;
        
        public event Action OnGameStart;
        public event Action OnGameEnd;
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
            NetworkPlayerManager.Instance.OnClientDisconnected += EndGame;

            BallController.Instance.OnScoreZoneReached += EndRound;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            NetworkPlayerManager.Instance.OnClientConnected -= TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected -= EndGame;
            
            BallController.Instance.OnScoreZoneReached -= EndRound;
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

        private void EndGame(ulong obj)
        {
            _currentGameState = GameState.End;
            OnGameEnd?.Invoke();
        }
    }

    internal enum GameState
    {
        WaitingForPlayers,
        Playing,
        End
    }
}