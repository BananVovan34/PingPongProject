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
            if (IsServer)
            {
                RoundEvents.RoundEnd += OnRoundEndServer;
                BallEvents.OnBallHitPaddle += OnBallHitPaddleServer;
                BallEvents.OnBallHitWall += OnBallHitWallServer;
            }
        }

        protected override void UnsubscribeEvents()
        {
            if (IsServer)
            {
                RoundEvents.RoundEnd -= OnRoundEndServer;
                BallEvents.OnBallHitPaddle -= OnBallHitPaddleServer;
                BallEvents.OnBallHitWall -= OnBallHitWallServer;
            }
        }

        private void OnRoundEndServer(byte playerId) => PlayGoalEffectsClientRpc();
        private void OnBallHitPaddleServer(Vector2 velocity) => BallHitPaddleClientRpc();
        private void OnBallHitWallServer(Vector2 velocity) => BallHitWallClientRpc();

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