using PingPong.Scripts.Gameplay.Ball;
using PingPong.Scripts.Gameplay.Particles;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Managers
{
    public class NetworkParticleManager : NetworkBehaviour
    {
        public static NetworkParticleManager Instance { get; private set; }

        [SerializeField] private EmitParticleController leftGoalParticles;
        [SerializeField] private EmitParticleController rightGoalParticles;
        
        private void Awake()
        {
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

            BallController.Instance.OnScoreZoneReached += EmitParticles;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            BallController.Instance.OnScoreZoneReached -= EmitParticles;
        }

        private void EmitParticles(string zoneTag)
        {
            if (zoneTag == "ScoreZoneLeft")
                EmitParticlesClientRpc(true);
            else if (zoneTag == "ScoreZoneRight")
                EmitParticlesClientRpc(false);
        }

        [ClientRpc]
        private void EmitParticlesClientRpc(bool isLeft)
        {
            if (isLeft)
            {
                leftGoalParticles.EmitRandomValue(16, 32 + 1);
            }
            else
                rightGoalParticles.EmitRandomValue(16, 32 + 1);
        }
    }
}