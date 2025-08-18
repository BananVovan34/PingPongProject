using System;
using UnityEngine;

public class ScoreManager : BaseManager
{
    private int _scorePlayer1 = 0;
    private int _scorePlayer2 = 0;
    
    public event Action<int, int, byte> OnScoreChanged;
    
    protected override void SubscribeEvents()
    {
        BallEvents.OnBallScored += AddScore;
    }

    protected override void UnsubscribeEvents()
    {
        BallEvents.OnBallScored -= AddScore;
    }

    public void AddScore(byte playerId, int score)
    {
        if (playerId == 1) _scorePlayer1 += score;
        if (playerId == 2) _scorePlayer2 += score;
        
        OnScoreChanged?.Invoke(_scorePlayer1, _scorePlayer2, playerId);
    }

    public void AddScore(byte playerId) { AddScore(playerId, 1); }
    
    public (int, int) GetScore() => (_scorePlayer1, _scorePlayer2);
}
