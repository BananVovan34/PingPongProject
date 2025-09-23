using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Ball;
using Gameplay.Game;
using Gameplay.Game.Round;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Managers
{
    public class NetworkGameManager : BaseNetworkGameManager
    {
        public Action OnReset;
        
        private GameStatus _gameStatus = GameStatus.WAITING_FOR_PLAYERS;
        public GameStatus GameStatus => _gameStatus;
        
        private readonly List<ulong> _connected = new List<ulong>();

        private void Start()
        {
            switch (GameStatement.Instance.CurrentStatus)
            {
                case Status.Host:
                    Debug.Log("Start Host");
                    NetworkManager.Singleton.StartHost();
                    break;

                case Status.Client:
                    Debug.Log("Start Client");
                    NetworkManager.Singleton.StartClient();
                    break;
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log("Client connected: " + clientId);
            _connected.Add(clientId);
            TryStartWhenReady();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log("Client disconnected: " + clientId);
            _connected.Remove(clientId);
        }

        private void TryStartWhenReady()
        {
            if (_connected.Count >= 2 && GameStatus == GameStatus.WAITING_FOR_PLAYERS)
            {
                _gameStatus = GameStatus.PLAYING;
                GameEvents.OnGameStart();
                OnGameStartClientRPC();
            }
        }

        [ClientRpc]
        private void OnGameStartClientRPC()
        {
            GameEvents.OnGameStart();
        }

        private void StartGame() {
            _gameStatus = GameStatus.PLAYING;
            
            RoundEvents.OnRoundStart();
        }

        private void EndGame(byte playerId)
        {
            StartCoroutine(CloseSession());
        }

        private IEnumerator CloseSession()
        {
            yield return new WaitForSeconds(5f);
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu");
        }

        public void OnScoreZoneReached(byte playerId)
        {
            RoundEvents.OnRoundEnd(playerId);
            OnRoundEndClientRPC(playerId);
            RoundEvents.OnRoundStart();
            OnRoundStartClientRPC();
        }

        [ClientRpc]
        private void OnRoundStartClientRPC()
        {
            RoundEvents.OnRoundStart();
        }

        protected override void SubscribeEvents()
        {
            GameEvents.GameStart += StartGame;
            BallEvents.OnBallScored += OnScoreZoneReached;
            RoundEvents.RoundStart += ResetRound;
            GameEvents.GameEnd += EndGame;
        }

        protected override void UnsubscribeEvents()
        {
            GameEvents.GameStart -= StartGame;
            BallEvents.OnBallScored -= OnScoreZoneReached;
            RoundEvents.RoundStart -= ResetRound;
            GameEvents.GameEnd -= EndGame;
        }
        
        [ClientRpc]
        private void OnRoundEndClientRPC(byte playerId)
        {
            RoundEvents.OnRoundEnd(playerId);
        }
    
        private void ResetRound() => OnReset?.Invoke();
    }
}
