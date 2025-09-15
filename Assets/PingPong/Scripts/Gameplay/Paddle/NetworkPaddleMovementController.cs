using Gameplay.Interface;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Paddle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkPaddleMovementController : NetworkBehaviour, IMoveableController
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float positionLerpSpeed = 20f;
        
        private NetworkVariable<Vector2> _position = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);
    
        private Rigidbody2D _rb;
        private Vector2 _targetPosition;

        private void Awake() => _rb = GetComponent<Rigidbody2D>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _position.Value = _rb.position;
            }

            _position.OnValueChanged += OnPositionChanged;
            _targetPosition = _rb.position;
        }

        public override void OnNetworkDespawn()
        {
            _position.OnValueChanged -= OnPositionChanged;
        }
        
        private void FixedUpdate()
        {
            if (IsServer)
            {
                _position.Value = _rb.position;
            }
            else
            {
                _rb.position = Vector2.Lerp(_rb.position, _targetPosition, Time.fixedDeltaTime * positionLerpSpeed);
            }
        }

        private void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            if (!IsServer)
            {
                transform.position = newPosition;
            }
        }

        [ServerRpc(RequireOwnership = true)]
        public void MoveServerRPC(Vector2 direction, ServerRpcParams rpcParams = default)
        {
            Debug.Log($"1Moving {direction}");
            if (!IsServer) return;
            Debug.Log($"2Moving {direction}");
            
            if (direction.sqrMagnitude > 1f) direction = direction.normalized;
            
            Debug.Log($"Moving {direction}");
            
            _rb.linearVelocity = direction * speed;
            
            _position.Value = _rb.position;
        }

        public void Move(Vector2 direction) =>
            MoveServerRPC(direction);
    }
}
