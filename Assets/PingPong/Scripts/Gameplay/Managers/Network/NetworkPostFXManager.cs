using Gameplay.Ball;
using Gameplay.Game.Round;
using Gameplay.PostFX;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkPostFXManager : BaseNetworkGameManager
    {
        [Header("PostFX References")]
        [SerializeField] private BloomIntensityController bloomIntensityController;
        [SerializeField] private ChromaticAberrationController chromaticAberrationController;

        protected override void SubscribeEvents()
        {
            RoundEvents.RoundEnd += OnRoundEndServer;
            BallEvents.OnBallHitPaddle += OnBallHitPaddleServer;
            BallEvents.OnBallHitWall += OnBallHitWallServer;
        }

        protected override void UnsubscribeEvents()
        {
            RoundEvents.RoundEnd -= OnRoundEndServer;
            BallEvents.OnBallHitPaddle -= OnBallHitPaddleServer;
            BallEvents.OnBallHitWall -= OnBallHitWallServer;
        }

        private void OnRoundEndServer(byte playerId) {
            if (!IsServer) return;
            PlayGoalEffectsClientRpc();
        }
        
        private void OnBallHitPaddleServer(Vector2 velocity)
        {
            if (!IsServer) return;
            BallHitPaddleClientRpc();
        }
        
        private void OnBallHitWallServer(Vector2 velocity)
        {
            if (!IsServer) return;
            BallHitWallClientRpc();
        }

        [ClientRpc]
        private void PlayGoalEffectsClientRpc()
        {
            bloomIntensityController.TriggerEffect();
            chromaticAberrationController.TriggerEffect();
        }

        [ClientRpc]
        private void BallHitPaddleClientRpc()
        {
            chromaticAberrationController.TriggerEffect(0.2f);
            bloomIntensityController.TriggerEffect(0.2f);
        }

        [ClientRpc]
        private void BallHitWallClientRpc()
        {
            chromaticAberrationController.TriggerEffect(0.1f);
            bloomIntensityController.TriggerEffect(0.1f);
        }
    }
}