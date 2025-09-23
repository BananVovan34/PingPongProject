using Gameplay.Game.Round;
using Gameplay.ParticleSystem;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkParticlesManager : BaseNetworkGameManager
    {
        [Header("Particles References")]
        [SerializeField] private EmitParticlesController goalParticlesPlayer1;
        [SerializeField] private EmitParticlesController goalParticlesPlayer2;

        protected override void SubscribeEvents()
        {
            RoundEvents.RoundEnd += OnRoundEndServer;
        }

        protected override void UnsubscribeEvents()
        {
            RoundEvents.RoundEnd -= OnRoundEndServer;
        }

        private void OnRoundEndServer(byte playerId)
        {
            if (!IsServer) return;
            PlayGoalEffectsClientRpc(playerId);
        }

        [ClientRpc]
        private void PlayGoalEffectsClientRpc(byte playerId)
        {
            int value = Random.Range(10, 20);

            if (playerId == 1) goalParticlesPlayer1.EmitParticles(value);
            if (playerId == 2) goalParticlesPlayer2.EmitParticles(value);
        }
    }
}