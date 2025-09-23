using System;
using PingPong.Scripts.Gameplay.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(IInputController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        private byte _playerId;
        
        private IInputController _inputController;
        private Rigidbody2D _rigidbody2D;

        private const float PlayerSpeed = 5f;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _inputController = GetComponent<IInputController>();
        }

        private void SetPlayerId(byte playerId)
        {
            if (playerId > 1)
                throw new ArgumentOutOfRangeException("playerId", "PlayerId must be between 0 and 1.");
            
            _playerId = playerId;
        }

        private void Update()
        {
            if (!IsOwner) return;
            Debug.Log(OwnerClientId + " : Update");
            
            var direction = _inputController.GetVerticalInput();
            
            Debug.Log(direction);
            
            SendInputServerRpc(direction);
        }
        
        [ServerRpc]
        private void SendInputServerRpc(Vector2 direction)
        {
            if (direction.sqrMagnitude > 1f)
                direction = direction.normalized;

            _rigidbody2D.linearVelocity = direction * PlayerSpeed;
        }
    }
}