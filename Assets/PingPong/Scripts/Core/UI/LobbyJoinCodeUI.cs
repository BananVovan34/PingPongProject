using System;
using PingPong.Scripts.Core.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PingPong.Scripts.Core.UI
{
    public class LobbyJoinCodeUI : MonoBehaviour
    {
        public static LobbyJoinCodeUI Instance { get; private set; }
        
        [SerializeField] private TextMeshProUGUI joinCodeText;
        [SerializeField] private TMP_InputField joinCodeInputField;
        
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
            LobbyManager.Instance.RelayJoinCodeChanged += UpdateJoinCodeText;
        }

        public void JoinWithCodeButtonPress()
        {
            JoinWithCodeButtonPressed?.Invoke(joinCodeInputField.text);
        }

        private void UpdateJoinCodeText(string joinCode)
        {
            joinCodeText.text = "Lobby join code: " + LobbyManager.Instance.RelayJoinCode;
        }
    }
}