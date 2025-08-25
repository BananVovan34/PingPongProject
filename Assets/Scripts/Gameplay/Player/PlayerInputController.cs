using Gameplay.Interface;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerInputController : MonoBehaviour, IInputController
    {
        private GameInput _input;

        private void Awake()
        {
            _input = new GameInput();
        }

        private void OnEnable() => _input.Enable();
        private void OnDisable() => _input.Disable();

        public Vector2 GetMovement() => _input.Gameplay.Movement.ReadValue<Vector2>();
        
    
    }
}