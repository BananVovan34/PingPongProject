using Gameplay.Ball;
using Gameplay.Camera;
using Gameplay.Game.Round;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Managers
{
    public class NetworkCameraManager : BaseNetworkGameManager
    {
        [Header("Camera References")]
        [SerializeField] private CameraShake cameraShake;

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
        private void OnBallHitPaddleServer(Vector2 velocity) => BallHitPaddleClientRpc(velocity);
        private void OnBallHitWallServer(Vector2 velocity) => BallHitWallClientRpc(velocity);

        [ClientRpc]
        private void PlayGoalEffectsClientRpc()
        {
            DoShake(0.1f, 0.2f);
        }

        [ClientRpc]
        private void BallHitPaddleClientRpc(Vector2 velocity)
        {
            float offset = Mathf.Sqrt(velocity.magnitude) * 0.01f;
            float duration = Random.Range(0.03f, 0.7f);
            DoShake(offset, duration);
        }

        [ClientRpc]
        private void BallHitWallClientRpc(Vector2 velocity)
        {
            float offset = Random.Range(0.015f, 0.035f);
            float duration = Random.Range(0.025f, 0.055f);
            DoShake(offset, duration);
        }

        private void DoShake(float offset, float duration)
        {
            cameraShake.StartShake(offset, duration);
        }
    }
}
