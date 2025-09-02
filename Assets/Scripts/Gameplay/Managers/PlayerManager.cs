using Gameplay.AI;
using Gameplay.Game;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] public GameObject playerPrefab;
        
        private Vector2 _initialLeftPlayerPosition = new Vector2(-10.35f, 0.0f);
        private Vector2 _initialRightPlayerPosition = new Vector2(10.35f, 0.0f);

        private void Start()
        {
            if (GameConfig.CurrentGamemode == Gamemode.LocalPve)
            {
                SpawnLocalPvePlayer(_initialLeftPlayerPosition);
                SpawnAI(_initialRightPlayerPosition);
            }
            
            if (GameConfig.CurrentGamemode == Gamemode.LocalPvp)
            {
                SpawnLocalPvpPlayer(_initialLeftPlayerPosition, true);
                SpawnLocalPvpPlayer(_initialRightPlayerPosition, false);
            }
        }

        private void SpawnAI(Vector2 position, float distanceCheck = 0.2f)
        {
            GameObject ai = Instantiate(playerPrefab, position, Quaternion.identity);
            ai.AddComponent<AIInputController>();
        }

        private void SpawnNetworkPlayer(Vector2 position) { }

        private void SpawnLocalPvePlayer(Vector2 position)
        {
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.AddComponent<PlayerInputController>();
            
            PlayerInputController playerInputController = player.GetComponent<PlayerInputController>();
            playerInputController.playerState = PlayerState.LocalPve;
        }
        
        private void SpawnLocalPvpPlayer(Vector2 position, bool isPlayer1)
        {
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.AddComponent<PlayerInputController>();
            
            PlayerInputController playerInputController = player.GetComponent<PlayerInputController>();
            playerInputController.isPlayer1 = isPlayer1;
            playerInputController.playerState = PlayerState.LocalPvp;
        }
    }
}