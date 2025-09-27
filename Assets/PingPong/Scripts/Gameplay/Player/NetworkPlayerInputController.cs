using PingPong.Scripts.Gameplay.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Player
{
    public class NetworkPlayerInputController : NetworkBehaviour, IInputController
    {
        private GameInput _gameInput;

        private void Awake()
        {
            _gameInput = new GameInput();
        }

        private void OnEnable()
        {
            _gameInput.Enable();
        }
        
        private void OnDisable()
        {
            _gameInput.Disable();
        }
        

        public Vector2 GetVerticalInput()
        {
            Debug.Log(OwnerClientId + " : GetVerticalInput");
            return _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        }
    }
}