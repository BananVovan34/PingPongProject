using Gameplay.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu.Managers
{
    public class UIManager : MonoBehaviour
    {
        public void OnGameLocalStart(bool isVersusAI)
        {
            if (isVersusAI) GameConfig.CurrentGamemode = Gamemode.LocalPve;
            else GameConfig.CurrentGamemode = Gamemode.LocalPvp;
            SceneManager.LoadScene("Gameplay");
        }

        public void OnExit() => Application.Quit();
    }
}
