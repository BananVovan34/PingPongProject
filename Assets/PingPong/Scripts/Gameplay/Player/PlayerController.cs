using System;
using System.Collections.Generic;
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

        private const float PlayerSpeed = 5f;
        private const float CorrectionThreshold = 0.5f;
        private const int BufferSize = 256;

        private int tickRate = 60;
        private float tickDeltaTime;
        private float accumulator;

        private int currentTick;

        private MovementData[] history = new MovementData[BufferSize];

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
            if (!IsOwner) return;

            accumulator += Time.deltaTime;

            while (accumulator >= tickDeltaTime)
            {
                accumulator -= tickDeltaTime;
                Tick();
            }
        }

        private void Tick()
        {
            currentTick++;

            Vector2 direction = _inputController.GetVerticalInput().normalized;
            Vector2 velocity = direction * PlayerSpeed;

            _rigidbody2D.linearVelocity = velocity;

            int index = currentTick % BufferSize;
            history[index] = new MovementData()
            {
                tick = currentTick,
                direction = direction,
                velocity = velocity,
                position = _rigidbody2D.position
            };

            SendInputServerRpc(direction, currentTick);
        }

        [ServerRpc]
        private void SendInputServerRpc(Vector2 direction, int tickSent, ServerRpcParams rpcParams = default) 
        {
            Vector2 velocity = direction * PlayerSpeed;
            _rigidbody2D.linearVelocity = velocity;

            Vector2 serverPos = _rigidbody2D.position;

            SendCorrectionClientRpc(serverPos, tickSent,
                new ClientRpcParams()
                {
                    Send = new ClientRpcSendParams()
                    {
                        TargetClientIds = new List<ulong>()
                        {
                            rpcParams.Receive.SenderClientId
                        }
                    }
                });
        }
        
        [ClientRpc]
        private void SendCorrectionClientRpc(Vector2 serverPos, int serverTick, ClientRpcParams parameters)
        {
            if (!IsOwner) return;

            int index = serverTick % BufferSize;
            Vector2 predicted = history[index].position;

            float error = Vector2.Distance(predicted, serverPos);

            if (error < CorrectionThreshold)
                return;

            DoRewind(serverTick, serverPos);
        }


        private void DoRewind(int rewindTick, Vector2 serverPos)
        {
            _rigidbody2D.position = serverPos;

            int tick = rewindTick;

            while (tick < currentTick)
            {
                tick++;

                int index = tick % BufferSize;
                Vector2 velocity = history[index].velocity;

                Vector2 newPos = _rigidbody2D.position + velocity * tickDeltaTime;

                _rigidbody2D.position = newPos;
                history[index].position = newPos;
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
    }
}
