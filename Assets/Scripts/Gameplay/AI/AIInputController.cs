using System;
using Gameplay.Interface;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.AI
{
    public class AIInputController : MonoBehaviour, IInputController
    {
        [SerializeField] private float distanceCheck = 0.2f;
    
        private Transform _ball;
        private InGameInputController _inputController;

        private void Awake()
        {
            _ball = GameObject.FindGameObjectWithTag("Ball").transform;
            _inputController = GetComponent<InGameInputController>();
        
            if (_ball == null)
                throw new NullReferenceException("ball is null");
        
            if (_inputController == null)
                throw new NullReferenceException("inputController is null");
        }

        public Vector2 GetMovement()
        {
            Vector2 paddlePosition = _inputController.GetPaddlePosition;
            
            if (_ball == null) return Vector2.zero;
        
            float difference = _ball.position.y - paddlePosition.y;
        
            if (Mathf.Abs(difference) < distanceCheck)
                return Vector2.zero;
        
            float direction = Mathf.Sign(difference);
        
            return new Vector2(0, direction);
        }
    }
}