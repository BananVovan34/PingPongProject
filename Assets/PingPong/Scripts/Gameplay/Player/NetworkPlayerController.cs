using Gameplay.Game;
using Gameplay.Interface;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Player
{
    public class NetworkPlayerController : NetworkBehaviour
    {
        [SerializeField] private GameObject paddle;
        
        [Header("Network send tuning")]
        [SerializeField] private float sendRate = 20f;
    
        private IMoveableController _moveableController;
        private IInputController _inputController;

        private float _sendTimer;

        private void Start()
        {
            _moveableController = paddle.GetComponent<IMoveableController>();
            _inputController = GetComponent<IInputController>();
        
            if (_moveableController == null)
                throw new System.NullReferenceException("No IMoveableController attached");

            if (_inputController == null)
                throw new System.NullReferenceException("No InputController attached");
        }
        
        public Vector2 GetPaddlePosition => 
            paddle.transform.position;
    
        void Update()
        {
            if (!IsOwner) return;
            
            Vector2? directionNullable = _inputController.GetMovement();
            
            if (!directionNullable.HasValue)
                return;
            
            Vector2 direction = directionNullable.Value;
            
            _sendTimer += Time.deltaTime;
            float interval = 1f / sendRate;

            if (_sendTimer >= interval)
            {
                Debug.Log("Sending paddle position");
                MovePaddleServerRPC(direction);
                _sendTimer = 0f;
            }
        }

        [ServerRpc(RequireOwnership = true)]
        private void MovePaddleServerRPC(Vector2 direction, ServerRpcParams rpcParams = default)
        {
            Debug.Log($"Moving paddle position {direction}");
            if (!IsServer) return;
            Debug.Log("Moving paddle position");
            
            ulong senderClientId = rpcParams.Receive.SenderClientId;

            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderClientId, out var netClient)) return;
            var playerObj = netClient.PlayerObject;
            if (playerObj == null) return;

            if (_moveableController != null)
            {
                _moveableController.Move(direction);
            }
        }
    }
}
