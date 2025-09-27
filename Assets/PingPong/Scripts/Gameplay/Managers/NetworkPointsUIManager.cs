using PingPong.Scripts.Gameplay.UI;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkPointsUIManager : NetworkBehaviour
    {
        public static NetworkPointsUIManager Instance { get; private set; }
        
        [SerializeField] private PointsText player1PointsText;
        [SerializeField] private PointsText player2PointsText;
        
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

            NetworkScoreManager.Instance.player1Points.OnValueChanged += (oldValue, newValue) =>
            {
                player1PointsText.UpdateText(newValue);
                player1PointsText.UpdateAnimation();
            };

            NetworkScoreManager.Instance.player2Points.OnValueChanged += (oldValue, newValue) =>
            {
                player2PointsText.UpdateText(newValue);
                player2PointsText.UpdateAnimation();
            };
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            NetworkScoreManager.Instance.player1Points.OnValueChanged -= (oldValue, newValue) =>
            {
                player1PointsText.UpdateText(newValue);
                player1PointsText.UpdateAnimation();
            };

            NetworkScoreManager.Instance.player2Points.OnValueChanged -= (oldValue, newValue) =>
            {
                player2PointsText.UpdateText(newValue);
                player2PointsText.UpdateAnimation();
            };
        }
    }
}