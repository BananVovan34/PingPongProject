using System;
using System.Collections.Generic;
using System.Text;
using PingPong.Scripts.Core.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace PingPong.Scripts.Core.UI
{
    public class MenuUIManager : MonoBehaviour
    {
        public static MenuUIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject joinLobbyWithCodePanel;
        [SerializeField] private GameObject connectingPanel;
        [SerializeField] private GameObject errorPanel;
        
        [Header("UI Buttons")]
        [SerializeField] private GameObject startGameButton;
        [SerializeField] private GameObject deleteLobbyButton;
        [SerializeField] private GameObject leaveLobbyButton;
        
        [Header("Join Code UI elements")]
        [SerializeField] private TextMeshProUGUI joinCodeText;
        [SerializeField] private TMP_InputField joinCodeInputField;
        
        [Header("Other")]
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TextMeshProUGUI playerListText;
        
        public Action<string> JoinWithCodeButtonPressed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            ShowMainMenu();
            
            var lobbyManager = LobbyManager.Instance;
            if (lobbyManager != null)
            {
                lobbyManager.OnLobbyCreatedSuccessfully += ShowLobby;
                lobbyManager.OnLobbyJoinedSuccessfully += ShowLobby;
                lobbyManager.OnLobbyJoinFailed += ShowError;
                lobbyManager.OnLobbyLeftSuccessfully += ShowMainMenu;
                LobbyManager.Instance.RelayJoinCodeChanged += UpdateJoinCodeText;
                LobbyManager.Instance.LobbyPlayersChanged += UpdatePlayerListText;
            }
        }

        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            lobbyPanel.SetActive(false);
            connectingPanel.SetActive(false);
            errorPanel.SetActive(false);
            joinLobbyWithCodePanel.SetActive(false);
        }
        
        public void ShowJoinLobbyWithCode()
        {
            mainMenuPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            connectingPanel.SetActive(false);
            errorPanel.SetActive(false);
            joinLobbyWithCodePanel.SetActive(true);
        }

        public void ShowLobby()
        {
            mainMenuPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            connectingPanel.SetActive(false);
            errorPanel.SetActive(false);
            joinLobbyWithCodePanel.SetActive(false);

            if (LobbyManager.Instance.IsPlayerHost)
            {
                startGameButton.SetActive(true);
                deleteLobbyButton.SetActive(true);
                leaveLobbyButton.SetActive(false);
            }
            else
            {
                startGameButton.SetActive(false);
                deleteLobbyButton.SetActive(false);
                leaveLobbyButton.SetActive(true);
            }
        }

        public void ShowConnecting()
        {
            mainMenuPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            connectingPanel.SetActive(true);
            errorPanel.SetActive(false);
            joinLobbyWithCodePanel.SetActive(false);
        }

        public void ShowError(string message)
        {
            mainMenuPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            connectingPanel.SetActive(false);
            errorPanel.SetActive(true);
            joinLobbyWithCodePanel.SetActive(false);

            errorText.text = message;
            Debug.LogWarning($"Error: {message}");
        }
        
        public void JoinWithCodeButtonPress()
        {
            JoinWithCodeButtonPressed?.Invoke(joinCodeInputField.text);
        }

        private void UpdateJoinCodeText(string joinCode)
        {
            joinCodeText.text = "Lobby join code: " + LobbyManager.Instance.RelayJoinCode;
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