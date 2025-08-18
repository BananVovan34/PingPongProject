using UnityEngine;

public class PostFXManager : MonoBehaviour
{
    public static PostFXManager Instance { get; private set; }
    
    [Header("PostFX References")]
    [SerializeField] private BloomIntensityController bloomIntensityController;
    [SerializeField] private ChromaticAberrationController chromaticAberrationController;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }
    
    private void OnEnable()
    {
        RoundEvents.RoundEnd += PlayGoalEffects;
        BallEvents.OnBallHitPaddle += HandleBallHitPaddle;
        BallEvents.OnBallHitWall += HandleBallHitWall;
    }

    private void OnDisable()
    {
        RoundEvents.RoundEnd -= PlayGoalEffects;
        BallEvents.OnBallHitPaddle -= HandleBallHitPaddle;
        BallEvents.OnBallHitWall -= HandleBallHitWall;
    }
    
    public void PlayGoalEffects(byte playerId)
    {
        bloomIntensityController.TriggerEffect();
        chromaticAberrationController.TriggerEffect();
    }
    
    private void HandleBallHitPaddle(Vector2 velocity)
    {
        TriggerChromaticAberrationEffects(0.2f);
        TriggerBloomEffects(0.2f);
    }
    
    private void HandleBallHitWall(Vector2 velocity)
    {
        TriggerChromaticAberrationEffects(0.1f);
        TriggerBloomEffects(0.1f);
    }
    
    public void TriggerBloomEffects()
    {
        bloomIntensityController.TriggerEffect();
    }

    public void TriggerBloomEffects(float duration)
    {
        bloomIntensityController.TriggerEffect(duration);
    }
    
    public void TriggerChromaticAberrationEffects(float duration)
    {
        chromaticAberrationController.TriggerEffect(duration);
    }
    
    public void TriggerChromaticAberrationEffects()
    {
        chromaticAberrationController.TriggerEffect();
    }
}