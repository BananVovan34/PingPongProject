using Gameplay.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu.Managers
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public void OnGameLocalStart(bool isVersusAI)
        {
            if (isVersusAI) GameStatement.Instance.CurrentGamemode = Gamemode.LocalPve;
            else GameStatement.Instance.CurrentGamemode = Gamemode.LocalPvp;
            SceneManager.LoadScene("Gameplay");
        }

        public void OnExit() => Application.Quit();
    }
}
