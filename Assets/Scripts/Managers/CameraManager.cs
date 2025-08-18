using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : BaseManager
{
    public static CameraManager Instance { get; private set; }
    
    [Header("Camera References")]
    [SerializeField] private CameraShake cameraShake;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }

    protected override void SubscribeEvents()
    {
        RoundEvents.RoundEnd += PlayGoalEffects;
        BallEvents.OnBallHitPaddle += HandleBallHitPaddle;
        BallEvents.OnBallHitWall += HandleBallHitWall;
    }

    protected override void UnsubscribeEvents()
    {
        RoundEvents.RoundEnd -= PlayGoalEffects;
        BallEvents.OnBallHitPaddle -= HandleBallHitPaddle;
        BallEvents.OnBallHitWall -= HandleBallHitWall;
    }

    private void PlayGoalEffects(byte playerId) => DoShake(0.1f, 0.2f);

    private void HandleBallHitPaddle(Vector2 velocity)
    {
        float offset = Mathf.Sqrt(velocity.magnitude) * 0.01f;
        float duration = Random.Range(0.03f, 0.7f);
        
        DoShake(offset, duration);
    }
    
    private void HandleBallHitWall(Vector2 velocity)
    {
        float offset = Random.Range(0.015f, 0.035f);
        float duration = Random.Range(0.025f, 0.055f);
        
        DoShake(offset, duration);
    }

    public void DoShake(float offset, float duration)
    {
        cameraShake.StartShake(offset, duration);
    }
}