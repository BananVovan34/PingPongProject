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
            if (!IsServer)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = false;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
                Reset();
            }
        }
        
        private void InitialLaunch()
        {
            Vector2 direction = Random.value > 0.5f ? Vector2.right  : Vector2.left;;
            direction.y = Random.Range(-maxInitialAngle / 100f, maxInitialAngle / 100f);
            rb.linearVelocity = direction * movementSpeed;
        
            BallEvents.BallLaunch();
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;
            
            if (collision.gameObject.CompareTag("Paddle"))
            {
                BallEvents.BallHitPaddle(rb.linearVelocity);
                BallHitPaddleClientRpc(rb.linearVelocity);
            }
        
            if (collision.gameObject.CompareTag("Wall"))
            {
                BallEvents.BallHitWall(rb.linearVelocity);
                BallHitWallClientRpc(rb.linearVelocity);
            }
        }
        
        [ClientRpc]
        private void BallHitPaddleClientRpc(Vector2 velocity)
        {
            BallEvents.BallHitPaddle(velocity);
        }

        [ClientRpc]
        private void BallHitWallClientRpc(Vector2 velocity)
        {
            BallEvents.BallHitWall(velocity);
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
                BallScoredClientRpc(1);
            }
            else if (collision.CompareTag("ScoreZoneRight"))
            {
                BallEvents.BallScored(2);
                BallScoredClientRpc(1);
            }
        }
        
        [ClientRpc]
        private void BallScoredClientRpc(byte playerId)
        {
            BallEvents.BallScored(playerId);
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
