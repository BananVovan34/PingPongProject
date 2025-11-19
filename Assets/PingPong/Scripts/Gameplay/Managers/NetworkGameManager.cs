using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PingPong.Scripts.Core.Lobby;
using PingPong.Scripts.Gameplay.Ball;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkGameManager : NetworkBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }
        
        private GameState _currentGameState = GameState.WaitingForPlayers;

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
            
            Debug.Log("OnNetworkSpawn");

            NetworkPlayerManager.Instance.OnClientConnected += TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected += ClientDisconnected;
            
            NetworkScoreManager.Instance.OnScoredPointsToWin += EndGame;

            BallController.Instance.OnScoreZoneReached += EndRound;
            
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            NetworkPlayerManager.Instance.OnClientConnected -= TryStartGame;
            NetworkPlayerManager.Instance.OnClientDisconnected -= ClientDisconnected;
            
            NetworkScoreManager.Instance.OnScoredPointsToWin -= EndGame;
            
            BallController.Instance.OnScoreZoneReached -= EndRound;
            
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            Debug.Log($"Scene loaded: {sceneName}. Clients completed: {clientsCompleted.Count}");
            StartCoroutine(WaitAndTryStartGame());
        }
        
        private IEnumerator WaitAndTryStartGame()
        {
            yield return new WaitForSeconds(2f);
            TryStartGame(123);
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
            Debug.Log("CountOfConnectedClients: " + countOfConnectedClients);
            
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
            StartCoroutine(EndGameSession());
        }

        private void NotifyLobbyManagerAboutGameEnd() =>
            NotifyGameEndClientRpc();

        [ClientRpc]
        private void NotifyGameEndClientRpc()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.EndGame();
            }
            NetworkManager.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private IEnumerator EndGameSession()
        {
            yield return new WaitForSeconds(5f);
            NotifyLobbyManagerAboutGameEnd();
            if (IsServer)
            {
                NetworkManager.Singleton.Shutdown();
                Application.Quit();
            }
        }
    }

    internal enum GameState
    {
        WaitingForPlayers,
        Playing,
        End
    }
}