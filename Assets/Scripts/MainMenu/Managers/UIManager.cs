using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject gamemodeSelectUI;

        private void Start()
        {
            gamemodeSelectUI.SetActive(false);
        }

        public void OnGamemodeSelect()
        {
            mainMenuUI.SetActive(false);
            gamemodeSelectUI.SetActive(true);
        }

        public void OnMainMenu()
        {
            gamemodeSelectUI.SetActive(false);
            mainMenuUI.SetActive(true);
        }

        public void OnGameLocalStart(bool isVersusAI)
        {
            if (isVersusAI) SceneManager.LoadScene("PlayerVSAI");
        }

        private void OnExit() => Application.Quit();
    }
}
