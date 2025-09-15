using Gameplay.Interface;
using UnityEngine;

namespace Gameplay.Paddle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LocalPaddleMovementController : MonoBehaviour, IMoveableController
    {
        [SerializeField] private float speed;
    
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 direction)
        {
            _rb.linearVelocity = direction * speed;
        }
    }
}
