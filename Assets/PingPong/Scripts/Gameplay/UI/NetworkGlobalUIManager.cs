using PingPong.Scripts.Gameplay.Managers;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.UI
{
    public class NetworkGlobalUIManager : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] pointsText;
        [SerializeField] private TextMeshProUGUI waitingForPlayersText;
        [SerializeField] private TextMeshProUGUI winnerText;

        private void Awake()
        {
            waitingForPlayersText.enabled = true;
            foreach (var text in pointsText)
            {
                text.enabled = false;
            }

            winnerText.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkGameManager.Instance.OnGameStart += OnGameStartHandler;
            NetworkGameManager.Instance.OnGameEnd += OnGameEndHandler;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            NetworkGameManager.Instance.OnGameStart -= OnGameStartHandler;
            NetworkGameManager.Instance.OnGameEnd -= OnGameEndHandler;
        }

        private void OnGameStartHandler()
        {
            Debug.Log("OnGameStartHandler");
            PointsTextEnableClientRpc();
            WaitingForPlayersTextDisableClientRpc();
        }

        private void OnGameEndHandler(byte winnerId)
        {
            WinnerTextEnableClientRpc(winnerId);
        }


        [ClientRpc]
        private void PointsTextEnableClientRpc()
        {
            Debug.Log("PointsTextEnableClientRpc");
            foreach (var text in pointsText)
                text.enabled = true;
        }

        [ClientRpc]
        private void WaitingForPlayersTextDisableClientRpc()
        {
            waitingForPlayersText.enabled = false;
        }

        [ClientRpc]
        private void WinnerTextEnableClientRpc(byte winnerId)
        {
            winnerText.text = "Player " + (winnerId + 1) + " won!";
            winnerText.enabled = true;
        }
    }
}