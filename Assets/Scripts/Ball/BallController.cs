using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] [Range(0.0f, 90.0f)] private float maxInitialAngle;

    private const float VelocityBoostPaddleHit = 1.2f;
    private const float VelocityBoostWallHit = 1.05f;
    
    private void Start()
    {
        InitialLaunch();
    }

    private void OnEnable()
    {
        RoundEvents.RoundStart += Reset;
        BallEvents.OnBallHitPaddle += HandleBallHitPaddle;
        BallEvents.OnBallHitWall += HandleBallHitWall;
    }
    
    private void OnDisable() 
    {
        RoundEvents.RoundStart -= Reset;
        BallEvents.OnBallHitPaddle -= HandleBallHitPaddle;
        BallEvents.OnBallHitWall -= HandleBallHitWall;
    }

    private void InitialLaunch()
    {
        Vector2 direction = Random.value > 0.5f ? Vector2.right  : Vector2.left;;
        direction.y = Random.Range(-maxInitialAngle / 100f, maxInitialAngle / 100f);
        rb.linearVelocity = direction * movementSpeed;
        
        BallEvents.BallLaunch();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            BallEvents.BallHitPaddle(rb.linearVelocity);
        }
        
        if (collision.gameObject.CompareTag("Wall"))
        {
            BallEvents.BallHitWall(rb.linearVelocity);
        }
    }

    private void HandleBallHitPaddle(Vector2 obj) => rb.linearVelocity *= VelocityBoostPaddleHit;
    private void HandleBallHitWall(Vector2 obj) => rb.linearVelocity *= VelocityBoostWallHit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ScoreZoneLeft"))
        {
            BallEvents.BallScored(1);
        }
        else if (collision.CompareTag("ScoreZoneRight"))
        {
            BallEvents.BallScored(2);
        }
    }
    
    private void ResetPosition()
    {
        transform.position = Vector2.zero;
    }

    private void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void Reset()
    {
        BallEvents.BallReset();
        
        ResetPosition();
        ResetVelocity();
        StartCoroutine(WaitBeforeLaunch());
    }

    private IEnumerator WaitBeforeLaunch()
    {
        yield return new WaitForSeconds(0.5f);
        InitialLaunch();
    }
}
