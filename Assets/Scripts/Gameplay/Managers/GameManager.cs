using System;
using Gameplay.Ball;
using Gameplay.Game.Round;
using UnityEngine;

namespace Gameplay.Managers
{
    public class GameManager : BaseManager
    {
        private Vector2 _initialPlayerPosition = new Vector2(10.35f, 0.0f);
    
        public Action OnReset;
    
        public void OnScoreZoneReached(byte playerId)
        {
            RoundEvents.OnRoundEnd(playerId);
            RoundEvents.OnRoundStart();
        }
    
        protected override void SubscribeEvents()
        {
            BallEvents.OnBallScored += OnScoreZoneReached;
            RoundEvents.RoundStart += ResetRound;
        }

        protected override void UnsubscribeEvents()
        {
            BallEvents.OnBallScored -= OnScoreZoneReached;
            RoundEvents.RoundStart -= ResetRound;
        }
    
        private void ResetRound() { OnReset?.Invoke(); }
    }
}
