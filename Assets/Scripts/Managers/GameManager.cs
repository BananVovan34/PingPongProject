using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : BaseManager
{
    public static GameManager Instance { get; private set; }

    private Vector2 _initialPlayerPosition = new Vector2(10.35f, 0.0f);
    
    public Action OnReset;
    
    public void OnScoreZoneReached(byte playerId)
    {
        RoundEvents.OnRoundEnd(playerId);
        RoundEvents.OnRoundStart();
    }

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
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
