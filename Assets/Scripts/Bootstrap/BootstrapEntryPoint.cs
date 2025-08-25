using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bootstrap
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        private IEnumerator Start()
        {
            if (false) yield return null;
            SceneManager.LoadScene("MainMenu");
        }
    }
}