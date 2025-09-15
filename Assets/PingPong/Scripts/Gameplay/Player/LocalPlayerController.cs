using Gameplay.Game;
using Gameplay.Interface;
using UnityEngine;

namespace Gameplay.Player
{
    public class LocalPlayerController : MonoBehaviour
    {
        [SerializeField] private GameObject paddle;
    
        private IMoveableController _moveableController;
        private IInputController _inputController;

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
            Vector2? directionNullable = _inputController.GetMovement();
            
            if (!directionNullable.HasValue)
                return;
            
            Vector2 direction = directionNullable.Value;
            _moveableController.Move(direction);
        }
    }
}
