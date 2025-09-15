using Gameplay.AI;
using Gameplay.Game;
using Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkPlayerManager : BaseNetworkGameManager
    {
        [SerializeField] public GameObject playerPrefab;
        
        private Vector2 _initialLeftPlayerPosition = new Vector2(-9.35f, 0.0f);
        private Vector2 _initialRightPlayerPosition = new Vector2(9.35f, 0.0f);
        
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
            SpawnNetworkPlayer(_initialLeftPlayerPosition, player1Id);
            SpawnNetworkPlayer(_initialRightPlayerPosition, player2Id);
        }

        private void SpawnNetworkPlayer(Vector2 position, ulong clientId)
        {
            var player = Instantiate(playerPrefab, position, Quaternion.identity);
            var netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);
        }
    }
}