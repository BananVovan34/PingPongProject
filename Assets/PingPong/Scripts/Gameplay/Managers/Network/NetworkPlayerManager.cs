using Gameplay.AI;
using Gameplay.Game;
using Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkPlayerManager : BaseNetworkGameManager
    {
        [Header("Settings")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;
        
        public Vector2 Player1SpawnPoint => player1SpawnPoint.position;
        public Vector2 Player2SpawnPoint => player2SpawnPoint.position;
        
        protected override void SubscribeEvents()
        {
            GameEvents.GameStart += NetworkGameStart;
        }

        protected override void UnsubscribeEvents()
        {
            GameEvents.GameStart -= NetworkGameStart;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void NetworkGameStart()
        {
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
                
            var player1Id = clientIds[0];
            var player2Id = clientIds[1];
            
            SpawnNetworkPlayer(Player1SpawnPoint, player1Id);
            SpawnNetworkPlayer(Player2SpawnPoint, player2Id);
        }

        private void SpawnNetworkPlayer(Vector2 position, ulong clientId)
        {
            var player = Instantiate(playerPrefab, position, Quaternion.identity);
            var netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);
        }
    }
}