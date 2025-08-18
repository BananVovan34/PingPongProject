using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EmitParticlesController collisionParticles;
    [SerializeField] private float movementSpeed;
    [SerializeField] [Range(0.0f, 90.0f)] private float maxInitialAngle;
    
    private void Start()
    {
        InitialLaunch();
        GameManager.Instance.OnReset += Reset;
    }

    private void InitialLaunch()
    {
        Vector2 direction = Random.value > 0.5f ? Vector2.right  : Vector2.left;;
        direction.y = Random.Range(-maxInitialAngle / 100f, maxInitialAngle / 100f);
        rb.linearVelocity = direction * movementSpeed;
    }

    private void ResetPosition()
    {
        transform.position = Vector2.zero;
    }

    private void Reset()
    {
        ResetPosition();
        InitialLaunch();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float offset = 0f;
        float duration = 0f;
        int value = 0;
        
        if (collision.gameObject.CompareTag("Paddle"))
        {
            Debug.Log("Paddle Collision");
            rb.linearVelocity *= 1.2f;
        
            value = Random.Range(3, 6);
        
            offset = Mathf.Sqrt(rb.linearVelocity.magnitude) * 0.02f;
            duration = Random.Range(0.05f, 0.1f);
            
            PostFXManager.Instance.TriggerChromaticAberrationEffects(0.2f);
            PostFXManager.Instance.TriggerBloomEffects(0.2f);
        }
        
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall Collision");
            
            rb.linearVelocity *= 1.05f;
        
            value = Random.Range(2, 4);
        
            offset = Random.Range(0.035f, 0.075f);
            duration = Random.Range(0.035f, 0.075f);
            
            PostFXManager.Instance.TriggerChromaticAberrationEffects(0.1f);
            PostFXManager.Instance.TriggerBloomEffects(0.1f);
        }
        
        CameraManager.Instance.DoShake(offset, duration);
        collisionParticles.EmitParticles(value);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ScoreZoneController scoreZoneController = collision.GetComponent<ScoreZoneController>();

        if (scoreZoneController)
        {
            float value = Random.Range(2, 4);
        
            float offset = Random.Range(0.075f, 0.15f);
            float duration = Random.Range(0.075f, 0.15f);
            
            CameraManager.Instance.DoShake(offset, duration);
            
            GameManager.Instance.OnScoreZoneReached(scoreZoneController.ID);
            Reset();
        }
    }
}
