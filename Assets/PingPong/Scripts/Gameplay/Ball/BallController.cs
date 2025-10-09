using System;
using PingPong.Scripts.Gameplay.Managers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PingPong.Scripts.Gameplay.Ball
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public class BallController : NetworkBehaviour
    {
        public static BallController Instance { get; private set; }
        
        [SerializeField] private float movementSpeed;
        [SerializeField, Range(0f, 90f)] private float maxInitialAngle = 45f;
        
        private Rigidbody2D _rigidbody2D;

        private const float MaxVelocity = 55f;
        private const float WallHitBoostVelocity = 1.05f;
        private const float PlayerHitBoostVelocity = 1.2f;

        public event Action OnReset;
        public event Action<float> OnBoostVelocity;
        public event Action<string> OnScoreZoneReached;
        public event Action OnBallHit;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            NetworkGameManager.Instance.OnRoundStart += Reset;
            NetworkGameManager.Instance.OnGameEnd += EndGameHandler;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            NetworkGameManager.Instance.OnRoundStart -= Reset;
            NetworkGameManager.Instance.OnGameEnd -= EndGameHandler;
        }

        /// <summary>
        /// Gives initial velocity
        /// </summary>
        private void InitLaunch()
        {
            Vector2 direction = Random.value > 0.5f ? Vector2.left : Vector2.right;
            direction.y = Random.Range(-maxInitialAngle / 100f, maxInitialAngle / 100f);
            
            Debug.Log(direction);
            
            _rigidbody2D.linearVelocity = direction * movementSpeed;
        }

        /// <summary>
        /// OnBallHit handler
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;
            
            if (collision.gameObject.CompareTag("Wall")) BoostVelocity(WallHitBoostVelocity);
            if (collision.gameObject.CompareTag("Player")) BoostVelocity(PlayerHitBoostVelocity);
            
            OnBallHit?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer) return;
            
            if (collision.gameObject.CompareTag("ScoreZoneLeft") || collision.gameObject.CompareTag("ScoreZoneRight"))
            {
                OnScoreZoneReached?.Invoke(collision.tag);
                EnableClientRPC(false);
            }
        }

        /// <summary>
        /// Client RPC method to enable GameObject
        /// </summary>
        /// <param name="value"></param>
        [ClientRpc]
        private void EnableClientRPC(bool value) =>
            enabled = value;

        /// <summary>
        /// Reset current transform and give initial velocity
        /// </summary>
        private void Reset()
        {
            var netTransform = GetComponent<NetworkTransform>();
            netTransform.Teleport(Vector3.zero, Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
            
            EnableClientRPC(true);
            if (IsServer)
            {
                InitLaunch();
            }
            else
            {
                _rigidbody2D.simulated = false;
            }
            
            OnReset?.Invoke();
        }

        /// <summary>
        /// Multiply velocity on value
        /// </summary>
        /// <param name="value"></param>
        private void BoostVelocity(float value)
        {
            _rigidbody2D.linearVelocity *= value;
            
            if (_rigidbody2D.linearVelocity.magnitude > MaxVelocity)
                _rigidbody2D.linearVelocity = _rigidbody2D.linearVelocity.normalized * MaxVelocity;
            
            OnBoostVelocity?.Invoke(value);
        }

        /// <summary>
        /// Ball is disabled after end game
        /// </summary>
        /// <param name="obj"></param>
        private void EndGameHandler(byte obj)
        {
            NetworkGameManager.Instance.OnRoundStart -= Reset;
            
            enabled = false;
        }
    }
}