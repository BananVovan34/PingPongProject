using Gameplay.Ball;
using Gameplay.Game.Round;
using Gameplay.PostFX;
using UnityEngine;

namespace Gameplay.Managers
{
    public class LocalPostFXManager : BaseGameManager
    {
        [Header("PostFX References")]
        [SerializeField] private BloomIntensityController bloomIntensityController;
        [SerializeField] private ChromaticAberrationController chromaticAberrationController;
    
        protected override void SubscribeEvents()
        {
            RoundEvents.RoundEnd += PlayGoalEffects;
            BallEvents.OnBallHitPaddle += HandleBallHitPaddle;
            BallEvents.OnBallHitWall += HandleBallHitWall;
        }

        protected override void UnsubscribeEvents()
        {
            RoundEvents.RoundEnd -= PlayGoalEffects;
            BallEvents.OnBallHitPaddle -= HandleBallHitPaddle;
            BallEvents.OnBallHitWall -= HandleBallHitWall;
        }
    
        public void PlayGoalEffects(byte playerId)
        {
            bloomIntensityController.TriggerEffect();
            chromaticAberrationController.TriggerEffect();
        }
    
        private void HandleBallHitPaddle(Vector2 velocity)
        {
            TriggerChromaticAberrationEffects(0.2f);
            TriggerBloomEffects(0.2f);
        }
    
        private void HandleBallHitWall(Vector2 velocity)
        {
            TriggerChromaticAberrationEffects(0.1f);
            TriggerBloomEffects(0.1f);
        }
    
        public void TriggerBloomEffects()
        {
            bloomIntensityController.TriggerEffect();
        }

        public void TriggerBloomEffects(float duration)
        {
            bloomIntensityController.TriggerEffect(duration);
        }
    
        public void TriggerChromaticAberrationEffects(float duration)
        {
            chromaticAberrationController.TriggerEffect(duration);
        }
    
        public void TriggerChromaticAberrationEffects()
        {
            chromaticAberrationController.TriggerEffect();
        }
    }
}