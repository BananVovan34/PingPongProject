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
            LocalScoreManager.OnScoreChanged += UpdateScoreUI;
            if (!IsServer) return;
            GameEvents.GameEnd += ShowWinnerText;
            GameEvents.GameStart += RemoveWaitingForPlayersText;
        }

        protected override void UnsubscribeEvents()
        {
            LocalScoreManager.OnScoreChanged -= UpdateScoreUI;
            if (!IsServer) return;
            GameEvents.GameEnd -= ShowWinnerText;
            GameEvents.GameStart -= RemoveWaitingForPlayersText;
        }
        
        private void ShowWinnerText(byte obj) => ShowWinnerTextClientRPC(obj);
        private void RemoveWaitingForPlayersText()
        {
            Debug.Log("Remove waiting for players");
            RemoveWaitingForPlayersTextClientRPC();
        }
        
        [ClientRpc]
        private void RemoveWaitingForPlayersTextClientRPC() => waitingForPlayersText.gameObject.SetActive(false);
        
        [ClientRpc]
        private void ShowWinnerTextClientRPC(byte obj)
        {
            winnerText.text = "Player " + obj + " won!";
            winnerText.gameObject.SetActive(true);
        }
    
        private void UpdateScoreUI(int score1, int score2, byte playerId)
        {
            scoreTextLeft.SetScore(score1);
            scoreTextRight.SetScore(score2);
        
            if (playerId == 1) scoreTextLeft.UpdateAnimation();
            if (playerId == 2) scoreTextRight.UpdateAnimation();
        }
    }
}