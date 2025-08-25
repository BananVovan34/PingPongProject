using Gameplay.Game.Round;
using Gameplay.ParticleSystem;
using UnityEngine;

namespace Gameplay.Managers
{
    public class ParticlesManager : BaseManager
    {
        [Header("Particles References")]
        [SerializeField] private EmitParticlesController goalParticlesPlayer1;
        [SerializeField] private EmitParticlesController goalParticlesPlayer2;
    
        protected override void SubscribeEvents()
        {
            RoundEvents.RoundEnd += PlayGoalEffects;
        }

        protected override void UnsubscribeEvents()
        {
            RoundEvents.RoundEnd -= PlayGoalEffects;
        }

        private void PlayGoalEffects(byte playerId) => EmitGoalParticles(playerId);
    
        public void EmitGoalParticles(byte playerId)
        {
            int value = Random.Range(10, 20);
        
            if (playerId == 1) goalParticlesPlayer1.EmitParticles(value);
            if (playerId == 2) goalParticlesPlayer2.EmitParticles(value);
        }
    }
}