using Gameplay.Game;
using Gameplay.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkUIManager : BaseNetworkGameManager
    {
        [Header("UI References")]
        [Header("Score UI References")]
        [SerializeField] private ScoreText scoreTextLeft;
        [SerializeField] private ScoreText scoreTextRight;
        [Header("Another UI References")]
        [SerializeField] private TextMeshProUGUI waitingForPlayersText;
        [SerializeField] private TextMeshProUGUI winnerText;

        private void Start()
        {
            waitingForPlayersText.gameObject.SetActive(true);
            winnerText.gameObject.SetActive(false);
        }
        
        protected override void SubscribeEvents()
        {
            NetworkScoreManager.OnScoreChanged += UpdateScoreUI;
            GameEvents.GameEnd += ShowWinnerText;
            GameEvents.GameStart += RemoveWaitingForPlayersText;
        }

        protected override void UnsubscribeEvents()
        {
            NetworkScoreManager.OnScoreChanged -= UpdateScoreUI;
            GameEvents.GameEnd -= ShowWinnerText;
            GameEvents.GameStart -= RemoveWaitingForPlayersText;
        }
        
        private void ShowWinnerText(byte obj)
        {
            Debug.Log("Show Winner Text");
            if (!IsServer) return;
            ShowWinnerTextClientRPC(obj);
        }
        
        private void RemoveWaitingForPlayersText()
        {
            if (!IsServer) return;
            Debug.Log("Remove Waiting For Players Text123");
            RemoveWaitingForPlayersTextClientRPC();
        }
        
        [ClientRpc]
        private void RemoveWaitingForPlayersTextClientRPC() {
            Debug.Log("Remove Waiting For Players Text2");
            waitingForPlayersText.gameObject.SetActive(false);
        }
        
        [ClientRpc]
        private void ShowWinnerTextClientRPC(byte obj)
        {
            Debug.Log("Show Winner Text1");
            winnerText.text = "Player " + obj + " won!";
            winnerText.gameObject.SetActive(true);
        }
    
        private void UpdateScoreUI(int score1, int score2, byte playerId)
        {
            Debug.Log("Update Score UI");
            scoreTextLeft.SetScore(score1);
            scoreTextRight.SetScore(score2);
        
            if (playerId == 1) scoreTextLeft.UpdateAnimation();
            if (playerId == 2) scoreTextRight.UpdateAnimation();
        }
    }
}