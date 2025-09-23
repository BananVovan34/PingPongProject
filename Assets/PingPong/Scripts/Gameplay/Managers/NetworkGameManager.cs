using System;
using System.Collections;
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

        private void OnEnable()
        {
            if (!IsServer) return;

            NetworkPlayerManager.Instance.OnClientConnected += TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected += EndGame;
        }

        private void OnDisable()
        {
            if (!IsServer) return;
            
            NetworkPlayerManager.Instance.OnClientConnected -= TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected -= EndGame;
        }

        private void TryStartGame(ulong obj)
        {
            int countOfConnectedClients = NetworkPlayerManager.Instance.ConnectedClients.Count;
            
            if (countOfConnectedClients >= 2 && _currentGameState == GameState.WaitingForPlayers)
                StartGame();
        }

        private void StartGame()
        {
            _currentGameState = GameState.Playing;
            OnGameStart?.Invoke();
        }

        private void EndRound()
        {
            OnRoundEnd?.Invoke();
            StartCoroutine(BeforeStartRound());
        }

        private IEnumerator BeforeStartRound()
        {
            yield return new WaitForSeconds(1f);
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