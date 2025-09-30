using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkPlayerManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;
        
        private Vector3 Player1SpawnPoint => player1SpawnPoint.position;
        private Vector3 Player2SpawnPoint => player2SpawnPoint.position;
        
        public static NetworkPlayerManager Instance { get; private set; }
        
        private List<ulong> _connectedClients = new List<ulong>();
        public IReadOnlyList<ulong> ConnectedClients => _connectedClients;
        
        private List<byte> _playerIds = new List<byte>();
        public IReadOnlyList<byte> PlayerIds => _playerIds;
        
        public event Action<ulong> OnClientConnected;
        public event Action<ulong> OnClientDisconnected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

                NetworkGameManager.Instance.OnGameStart += SpawnPlayers;
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
                
                NetworkGameManager.Instance.OnGameStart += SpawnPlayers;
            }
            
            _connectedClients.Clear();
            _playerIds.Clear();
        }

        private void SpawnPlayers()
        {
            if (_connectedClients.Count < 2) return;
            
            for (int i = 0; i < 2; i++)
            {
                var currentClientId = _connectedClients[i];
                
                SpawnNetworkPlayer((byte)i, currentClientId);
                
                _playerIds.Add((byte)i);
            }
        }

        private void SpawnNetworkPlayer(byte playerId, ulong clientId)
        {
            Debug.Log("Spawning player " + playerId);
            
            Vector3 position = playerId == 0 ? Player1SpawnPoint : Player2SpawnPoint;
            
            GameObject playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
        }

        private void ClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");
            if (!_connectedClients.Contains(clientId)) _connectedClients.Add(clientId);
            OnClientConnected?.Invoke(clientId);
        }
        
        private void ClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
            _connectedClients.Remove(clientId);
            OnClientDisconnected?.Invoke(clientId);
        }
    }
}