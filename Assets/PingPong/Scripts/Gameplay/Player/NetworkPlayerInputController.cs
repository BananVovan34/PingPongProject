using Gameplay.Interface;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Player
{
    public class NetworkPlayerInputController : NetworkBehaviour, IInputController
    {
        private GameInput _input;

        private void Awake() =>
            _input = new GameInput();

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _input.Enable();
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsOwner) _input.Disable();
        }

        public Vector2? GetMovement()
        {
            if (!IsOwner) return null;
            return _input.Gameplay.Movement.ReadValue<Vector2>();
        }
    }
}