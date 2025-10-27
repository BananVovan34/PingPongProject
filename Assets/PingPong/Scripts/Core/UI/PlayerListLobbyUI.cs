using System.Collections.Generic;
using System.Text;
using PingPong.Scripts.Core.Lobby;
using TMPro;
using UnityEngine;

namespace PingPong.Scripts.Core.UI
{
    public class PlayerListLobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerListText;

        private void Start()
        {
            LobbyManager.Instance.LobbyPlayersChanged += UpdatePlayerListText;
        }

        private void UpdatePlayerListText(List<string> playerList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Player List:");

            foreach (var player in playerList)
            {
                stringBuilder.AppendLine(player);
            }
            
            playerListText.text = stringBuilder.ToString();
        }
    }
}