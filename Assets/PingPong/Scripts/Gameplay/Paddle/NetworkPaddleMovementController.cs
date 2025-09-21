using Gameplay.Interface;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Paddle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkPaddleMovementController : NetworkBehaviour, IMoveableController
    {
        [SerializeField] private float speed = 5f;
        private Rigidbody2D _rb;
        private Vector2 _targetPosition;

        private void Awake() => _rb = GetComponent<Rigidbody2D>();
        
        public void Move(Vector2 direction)
        {
            MoveServerRPC(direction);
        }

        [ServerRpc]
        private void MoveServerRPC(Vector2 direction)
        {
            if (direction.sqrMagnitude > 1f) direction = direction.normalized;
            
            Debug.Log($"{OwnerClientId} Moving {direction}");
            
            _rb.linearVelocity = direction * speed;
        }
    }
}
