using Gameplay.Interface;
using UnityEngine;

namespace Gameplay.Player
{
    public class InGameInputController : MonoBehaviour
    {
        [SerializeField] private GameObject paddle;
    
        private IMoveableController _moveableController;
        private IInputController _inputController;

        private void Awake()
        {
            _moveableController = paddle.GetComponent<IMoveableController>();
            _inputController = GetComponent<IInputController>();
        
            if (_moveableController == null)
                throw new System.NullReferenceException("No IMoveableController attached");

            if (_inputController == null)
                throw new System.NullReferenceException("No InputController attached");
        
        }
    
        void Update()
        {
            Vector2 direction = _inputController.GetMovement();
        
            _moveableController.Move(direction);
        }
    }
}
