using System;
using PingPong.Scripts.Core;
using PingPong.Scripts.Gameplay.Interfaces;
using PingPong.Scripts.Gameplay.Managers;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(IInputController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : NetworkBehaviour
    {
        private byte _playerId;
        
        private IInputController _inputController;
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;

        private const float PlayerSpeed = 5f;
        private const float MaxDeltaDistance = 1f;
        private const float CorrectionLerp = 10f;
        
        private NetworkVariable<Vector2> _serverPosition = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _inputController = GetComponent<IInputController>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SetupPlayer();

            _serverPosition.OnValueChanged += OnServerPositionChanged;
        }

        public override void OnDestroy()
        {
            _serverPosition.OnValueChanged -= OnServerPositionChanged;
        }

        private void OnServerPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            if (!IsOwner)
            {
                transform.position = Vector2.Lerp(transform.position, newPosition, Time.deltaTime * CorrectionLerp);
                return;
            }

            float offset = Vector2.Distance(transform.position, newPosition);
            if (offset > 0.1f)
            {
                transform.position = Vector2.Lerp(transform.position, newPosition, Time.deltaTime * CorrectionLerp);
            }
        }
        
        private void SetupPlayer()
        {
            float edgeMultiplier;
            if (IsHost)
            {
                if (IsOwner)
                {
                    _spriteRenderer.color = Color.cyan;
                    edgeMultiplier = -1f;
                }
                else
                {
                    _spriteRenderer.color = Color.grey;
                    edgeMultiplier = 1f;
                }
            }
            else
            {
                if (IsOwner)
                {
                    _spriteRenderer.color = Color.cyan;
                    edgeMultiplier = 1f;
                }
                else
                {
                    _spriteRenderer.color = Color.grey;
                    edgeMultiplier = -1f;
                }
            }
            
            transform.position = NetworkPlayerManager.Instance.Player1SpawnPoint * edgeMultiplier;
        }

        private void SetPlayerId(byte playerId)
        {
            if (playerId > 1)
                throw new ArgumentOutOfRangeException("playerId", "PlayerId must be between 0 and 1.");
            
            _playerId = playerId;
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            var direction = _inputController.GetVerticalInput().normalized;

            _rigidbody2D.linearVelocity = direction * PlayerSpeed;
        }

        /// <summary>
        /// Server movement processing
        /// </summary>
        [ServerRpc]
        private void SendVelocityServerRpc(Vector2 direction, Vector2 clientPosition, Vector2 clientVelocity)
        {
            Vector2 serverPredictedPos = _rigidbody2D.position + clientVelocity * Time.fixedDeltaTime;

            float dist = Vector2.Distance(clientPosition, serverPredictedPos);
            if (dist > MaxDeltaDistance)
            {
                Debug.LogWarning($"[Server] Client {OwnerClientId} desynced ({dist:F2}), correcting.");
                serverPredictedPos = Vector2.Lerp(_rigidbody2D.position, clientPosition, 0.5f);
            }

            _rigidbody2D.MovePosition(serverPredictedPos);
            _rigidbody2D.linearVelocity = clientVelocity;
            _serverPosition.Value = serverPredictedPos;
        }
    }
}