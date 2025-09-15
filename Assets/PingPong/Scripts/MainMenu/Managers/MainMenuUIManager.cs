using Gameplay.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu.Managers
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public void OnGameLocalStart(bool isVersusAI)
        {
            if (isVersusAI) GameStatement.Instance.CurrentStatus = Status.LocalPve;
            else GameStatement.Instance.CurrentStatus = Status.LocalPvp;
            SceneManager.LoadScene("LocalGame");
        }

        public void OnStartHost()
        {
            GameStatement.Instance.CurrentStatus = Status.Host;
            SceneManager.LoadScene("MultiplayerGame");
        }

        public void OnClientJoin()
        {
            GameStatement.Instance.CurrentStatus = Status.Client;
            SceneManager.LoadScene("MultiplayerGame");
        }

        public void OnExit() => Application.Quit();
    }
}
