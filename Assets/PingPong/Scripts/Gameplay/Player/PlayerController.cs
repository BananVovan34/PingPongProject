using System;
using System.Collections.Generic;
using System.Security.Principal;
using PingPong.Scripts.Core;
using PingPong.Scripts.Core.Network;
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
        private IInputController _inputController;
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;
        
        private byte _playerId;

        private int tickRate = 60;
        private int currentTick;
        private float time;
        private float tickDeltaTime;

        private const int BufferSize = 1024;
        private MovementData[] clientMovementData = new MovementData[BufferSize];

        private const float PlayerSpeed = 5f;
        private const float MaxDeltaDistance = 1f;
        private const float CorrectionLerp = 10f;

        private bool isTeleporting;
        private bool lastIsTeleporting;
        
        private NetworkVariable<Vector2> _serverPosition = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _inputController = GetComponent<IInputController>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            tickDeltaTime = 1f / tickRate;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SetupPlayer();
        }
        
        private void Update()
        {
            time += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            while (time > tickDeltaTime)
            {
                currentTick++;
                time -= tickDeltaTime;

                Move();
            }
        }

        private void Move()
        {
            var direction = _inputController.GetVerticalInput().normalized;
            var velocity = direction * PlayerSpeed;

            _rigidbody2D.linearVelocity = velocity;

            clientMovementData[currentTick % BufferSize] = new MovementData()
            {
                tick = currentTick,
                direction = direction,
                position = _rigidbody2D.position
            };
            
            Physics2D.SyncTransforms();
            
            MoveServerRpc(
                clientMovementData[currentTick % BufferSize],
                clientMovementData[(currentTick - 1) % BufferSize],
                new ServerRpcParams()
                {
                    Receive = new ServerRpcReceiveParams() { SenderClientId = OwnerClientId }
                }
                );
        }
        
        [ServerRpc]
        private void MoveServerRpc(MovementData currentMovementData, MovementData previousMovementData, ServerRpcParams serverRpcParams)
        {
            Vector2 startPosition = _rigidbody2D.position;
            Vector2 moveVector = previousMovementData.direction.normalized * PlayerSpeed;
            Physics.simulationMode = SimulationMode.Script;
            _rigidbody2D.position = previousMovementData.position;
            _rigidbody2D.linearVelocity = moveVector;
            Physics.Simulate(Time.fixedDeltaTime);
            Vector2 correctPosition = _rigidbody2D.position;
            _rigidbody2D.position = startPosition;
            Physics.simulationMode = SimulationMode.FixedUpdate;

            if (Vector2.Distance(correctPosition, currentMovementData.position) > MaxDeltaDistance)
            {
                Debug.LogWarning("Player is moving too fast");
                
                ReconciliateClientRPC(
                    currentMovementData.tick,
                    new ClientRpcParams()
                    {
                        Send = new ClientRpcSendParams()
                        {
                            TargetClientIds = new List<ulong> { serverRpcParams.Receive.SenderClientId }
                        }
                    }
                    );
            }
            
            Vector2 endPosition = previousMovementData.position;
        }
        
        [ClientRpc]
        private void ReconciliateClientRPC(int activationTick, ClientRpcParams parameters)
        {
            Vector2 correctPosition = clientMovementData[(activationTick - 1) % BufferSize].position;

            Physics.simulationMode = SimulationMode.Script;
            while (activationTick <= currentTick)
            {
                Vector2 moveVector = clientMovementData[(activationTick - 1) % BufferSize].direction.normalized * PlayerSpeed;
                transform.position = correctPosition;
                _rigidbody2D.linearVelocity = moveVector;
                Physics.Simulate(Time.fixedDeltaTime);
                correctPosition = transform.position;
                clientMovementData[activationTick % BufferSize].position = correctPosition;
                activationTick++;
            }
            Physics.simulationMode = SimulationMode.FixedUpdate;

            transform.position = correctPosition;
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
    }
}