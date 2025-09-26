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
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            NetworkGameManager.Instance.OnRoundStart -= Reset;
        }

        private void InitLaunch()
        {
            Vector2 direction = Random.value > 0.5f ? Vector2.left : Vector2.right;
            direction.y = Random.Range(-maxInitialAngle / 100f, maxInitialAngle / 100f);
            
            Debug.Log(direction);
            
            _rigidbody2D.linearVelocity = direction * movementSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;
            
            if (collision.gameObject.CompareTag("Wall")) BoostVelocity(WallHitBoostVelocity);
            if (collision.gameObject.CompareTag("Player")) BoostVelocity(PlayerHitBoostVelocity);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer) return;
            
            if (collision.gameObject.CompareTag("ScoreZoneLeft") || collision.gameObject.CompareTag("ScoreZoneRight"))
                OnScoreZoneReached?.Invoke(collision.tag);
        }

        private void Reset()
        {
            transform.position = Vector2.zero;
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

        private void BoostVelocity(float obj)
        {
            _rigidbody2D.linearVelocity *= obj;
            
            if (_rigidbody2D.linearVelocity.magnitude > MaxVelocity)
                _rigidbody2D.linearVelocity = _rigidbody2D.linearVelocity.normalized * MaxVelocity;
            
            OnBoostVelocity?.Invoke(obj);
        }
    }
}