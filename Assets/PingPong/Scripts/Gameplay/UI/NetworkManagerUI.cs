using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.UI
{
    public class NetworkManagerUI : MonoBehaviour
    {
        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
        }
        
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }
        
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}