using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PingPong.Scripts.Core.EntryPoint
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        [SerializeField] private string clientNextSceneName = "MainMenu";
        [SerializeField] private string serverNextSceneName = "ServerScene";
        
        private void Start()
        {
            if (System.Environment.GetCommandLineArgs().Any(arg => arg.Equals("-port")))
            {
                Debug.Log("Starting a server");
                SceneManager.LoadScene(serverNextSceneName);
            }
            else
            {
                Debug.Log("Starting a client");
                SceneManager.LoadScene(clientNextSceneName);
            }
        }
    }
}