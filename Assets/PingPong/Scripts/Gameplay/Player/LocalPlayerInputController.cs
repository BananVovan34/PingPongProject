using Gameplay.Interface;
using UnityEngine;

namespace Gameplay.Player
{
    public class LocalPlayerInputController : MonoBehaviour, IInputController
    {
        [SerializeField] public PlayerState playerState = PlayerState.LocalPve;
        [SerializeField] public bool isPlayer1 = true;
        
        private GameInput _input;

        private void Awake()
        {
            _input = new GameInput();
        }

        private void OnEnable() => _input.Enable();
        private void OnDisable() => _input.Disable();

        public Vector2? GetMovement()
        {
            if (playerState == PlayerState.LocalPve) return GetLocalPveMovement();
            if (playerState == PlayerState.LocalPvp) return GetLocalPvpMovement();
            return GetMultiplayerMovement();
        }

        private Vector2 GetMultiplayerMovement() => throw new System.NotImplementedException();

        private Vector2 GetLocalPveMovement() => _input.Gameplay.Movement.ReadValue<Vector2>();

        private Vector2 GetLocalPvpMovement()
        {
            if (isPlayer1) return _input.Gameplay.Player1.ReadValue<Vector2>();
            return _input.Gameplay.Player2.ReadValue<Vector2>();
        }
    }

    public enum PlayerState
    {
        LocalPve,
        LocalPvp
    }
}