using System;
using UnityEngine;

public static class BallEvents
{
    public static event Action<Vector2> OnBallHit; 
    public static event Action<Vector2> OnBallHitPaddle;
    public static event Action<Vector2> OnBallHitWall;
    public static event Action<byte> OnBallScored;
    
    private static void OnOnBallHit(Vector2 hitPosition) => OnBallHit?.Invoke(hitPosition);
    public static void BallHitPaddle(Vector2 velocity) => OnBallHitPaddle?.Invoke(velocity);
    public static void BallHitWall(Vector2 velocity) => OnBallHitWall?.Invoke(velocity);
    public static void BallScored(byte playerId) => OnBallScored?.Invoke(playerId);
}