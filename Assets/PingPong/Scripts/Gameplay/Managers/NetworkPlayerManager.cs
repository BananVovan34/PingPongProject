using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Netcode;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkPlayerManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;
        
        public Vector3 Player1SpawnPoint => player1SpawnPoint.position;
        public Vector3 Player2SpawnPoint => player2SpawnPoint.position;
        
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
            if (!IsServer) return;
            
            Debug.Log($"Network Spawned: {gameObject.name}");
            
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

            NetworkGameManager.Instance.OnGameStart += SpawnPlayers;

            FirstLaunchHandler();
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
        
        private void FirstLaunchHandler()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!_connectedClients.Contains(clientId))
                {
                    _connectedClients.Add(clientId);
                    OnClientConnected?.Invoke(clientId);
                }
            }
            Debug.Log($"FirstLaunchHandler {_connectedClients.Count}");
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
            
            //Vector3 position = playerId == 0 ? Player1SpawnPoint : Player2SpawnPoint;
            
            GameObject playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
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