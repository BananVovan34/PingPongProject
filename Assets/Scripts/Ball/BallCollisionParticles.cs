using UnityEngine;

public class BallCollisionParticles : EmitParticlesController
{
    private void OnEnable()
    {
        BallEvents.OnBallHitPaddle += HandleBallHitPaddle;
        BallEvents.OnBallHitWall += HandleBallHitWall;
    }

    private void OnDisable()
    {
        BallEvents.OnBallHitPaddle -= HandleBallHitPaddle;
        BallEvents.OnBallHitWall -= HandleBallHitWall;
    }

    private void HandleBallHitWall(Vector2 obj)
    {
        int value = Random.Range(2, 4);
        EmitParticles(value);
    }

    private void HandleBallHitPaddle(Vector2 obj)
    {
        int value = Random.Range(3, 6);
        EmitParticles(value);
    }
}