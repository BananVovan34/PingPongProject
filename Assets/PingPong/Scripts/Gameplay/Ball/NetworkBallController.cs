using System.Collections;
using Gameplay.Game.Round;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Ball
{
    public class NetworkBallController : NetworkBehaviour
    {
        [Header("Ball Settings")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float movementSpeed;
        [SerializeField] [Range(0.0f, 90.0f)] private float maxInitialAngle;

        private const float VelocityBoostPaddleHit = 1.2f;
        private const float VelocityBoostWallHit = 1.05f;
        private const float MaxVelocity = 55.0f;
        
        [Header("Interpolation")]
        [SerializeField] private float predictionFactor = 0.1f;
        [SerializeField] private float positionLerpSpeed = 15f;
        [SerializeField] private float velocityLerpSpeed = 10f;
        
        private NetworkVariable<Vector2> _position = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);
        private NetworkVariable<Vector2> _velocity = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);
        
        private Vector2 _targetPosition;
        private Vector2 _targetVelocity;

        private void OnEnable()
        {
            RoundEvents.RoundStart += Reset;
            BallEvents.OnBallHitPaddle += HandleBallHitPaddle;
            BallEvents.OnBallHitWall += HandleBallHitWall;
        }
    
        private void OnDisable() 
        {
            RoundEvents.RoundStart -= Reset;
            BallEvents.OnBallHitPaddle -= HandleBallHitPaddle;
            BallEvents.OnBallHitWall -= HandleBallHitWall;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Reset();
            }

            _position.OnValueChanged += OnPositionChanged;
            _velocity.OnValueChanged += OnVelocityChanged;

            _targetPosition = rb.position;
            _targetVelocity = rb.linearVelocity;
        }

        public override void OnNetworkDespawn()
        {
            _position.OnValueChanged -= OnPositionChanged;
            _velocity.OnValueChanged -= OnVelocityChanged;
        }
        
        private void FixedUpdate()
        {
            if (IsServer)
            {
                // Сервер пушит данные
                _position.Value = rb.position;
                _velocity.Value = rb.linearVelocity;
            }
            else
            {
                Vector2 predictedPos = _targetPosition + _targetVelocity * predictionFactor;
                rb.position = Vector2.Lerp(rb.position, predictedPos, Time.fixedDeltaTime * positionLerpSpeed);
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, _targetVelocity, Time.fixedDeltaTime * velocityLerpSpeed);
            }
        }
        
        private void InitialLaunch()
        {
            Vector2 direction = Random.value > 0.5f ? Vector2.right  : Vector2.left;;
            direction.y = Random.Range(-maxInitialAngle / 100f, maxInitialAngle / 100f);
            rb.linearVelocity = direction * movementSpeed;
        
            BallEvents.BallLaunch();
        }
        
        private void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            if (!IsServer)
                rb.position = newPosition;
        }

        private void OnVelocityChanged(Vector2 oldVelocity, Vector2 newVelocity)
        {
            if (!IsServer)
                rb.linearVelocity = newVelocity;
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;
            
            if (collision.gameObject.CompareTag("Paddle"))
            {
                BallEvents.BallHitPaddle(rb.linearVelocity);
            }
        
            if (collision.gameObject.CompareTag("Wall"))
            {
                BallEvents.BallHitWall(rb.linearVelocity);
            }
        }

        private void HandleBallHitPaddle(Vector2 obj)
        {
            rb.linearVelocity *= VelocityBoostPaddleHit;
            
            if (rb.linearVelocity.magnitude > MaxVelocity) rb.linearVelocity = rb.linearVelocity.normalized * MaxVelocity;
        }

        private void HandleBallHitWall(Vector2 obj)
        {
            rb.linearVelocity *= VelocityBoostWallHit;
            
            if (rb.linearVelocity.magnitude > MaxVelocity) rb.linearVelocity = rb.linearVelocity.normalized * MaxVelocity;
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer) return;
            
            if (collision.CompareTag("ScoreZoneLeft"))
            {
                BallEvents.BallScored(1);
            }
            else if (collision.CompareTag("ScoreZoneRight"))
            {
                BallEvents.BallScored(2);
            }
        }
    
        private void ResetPosition() => rb.position = Vector2.zero;
        private void ResetVelocity() => rb.linearVelocity = Vector2.zero;

        private void Reset()
        {
            BallEvents.BallReset();
        
            ResetPosition();
            ResetVelocity();
            StartCoroutine(WaitBeforeLaunch());
        }

        private IEnumerator WaitBeforeLaunch()
        {
            yield return new WaitForSeconds(0.5f);
            InitialLaunch();
        }
    }
}
