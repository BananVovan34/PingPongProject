using Gameplay.AI;
using Gameplay.Game;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Managers
{
    public class LocalPlayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;
        
        public Vector2 Player1SpawnPoint => player1SpawnPoint.position;
        public Vector2 Player2SpawnPoint => player2SpawnPoint.position;

        private void Start()
        {
            if (GameStatement.Instance.CurrentStatus == Status.LocalPve)
            {
                SpawnLocalPvePlayer(Player1SpawnPoint);
                SpawnAI(Player2SpawnPoint);
            }
            
            if (GameStatement.Instance.CurrentStatus == Status.LocalPvp)
            {
                SpawnLocalPvpPlayer(Player1SpawnPoint, true);
                SpawnLocalPvpPlayer(Player2SpawnPoint, false);
            }
        }

        private void SpawnAI(Vector2 position, float distanceCheck = 0.2f)
        {
            GameObject ai = Instantiate(playerPrefab, position, Quaternion.identity);
            ai.AddComponent<AIInputController>();
        }
        private void SpawnLocalPvePlayer(Vector2 position)
        {
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.AddComponent<LocalPlayerInputController>();
            
            LocalPlayerInputController localPlayerInputController = player.GetComponent<LocalPlayerInputController>();
            localPlayerInputController.playerState = PlayerState.LocalPve;
        }
        
        private void SpawnLocalPvpPlayer(Vector2 position, bool isPlayer1)
        {
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.AddComponent<LocalPlayerInputController>();
            
            LocalPlayerInputController localPlayerInputController = player.GetComponent<LocalPlayerInputController>();
            localPlayerInputController.isPlayer1 = isPlayer1;
            localPlayerInputController.playerState = PlayerState.LocalPvp;
        }
    }
}