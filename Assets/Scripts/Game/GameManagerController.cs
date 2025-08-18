using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerController : MonoBehaviour
{
    public static GameManagerController Instance { get; private set; }
    
    [Header("Camera References")]
    [SerializeField] private CameraShake cameraShake;
    
    [Header("UI References")]
    [SerializeField] private ScoreText scoreTextLeft;
    [SerializeField] private ScoreText scoreTextRight;
    
    [Header("Particles References")]
    [SerializeField] private EmitParticlesController goalParticlesPlayer1;
    [SerializeField] private EmitParticlesController goalParticlesPlayer2;
    
    [Header("PostProcessing References")]
    [SerializeField] private BloomIntensityController bloomIntensityController;
    [SerializeField] private ChromaticAberrationController chromaticAberrationController;

    private Vector2 _initialPlayerPosition = new Vector2(10.35f, 0.0f);
    
    private int _scorePlayer1 = 0;
    private int _scorePlayer2 = 0;
    
    public Action onReset;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }

    public void OnScoreZoneReached(byte id)
    {
		onReset?.Invoke();

        if (id == 1) _scorePlayer1++;
        if (id == 2) _scorePlayer2++;
        
        UpdateScoreText();
        UpdateScoreTextAnimation(id);
        EmitGoalParticles(id);
        TriggerBloomEffects();
        TriggerChromaticAberrationEffects();
    }

    public void DoShake(float offset, float duration)
    {
        cameraShake.StartShake(offset, duration);
    }

    private void UpdateScoreText()
    {
        scoreTextLeft.SetScore(_scorePlayer1);
        scoreTextRight.SetScore(_scorePlayer2);
    }

    private void UpdateScoreTextAnimation(byte id)
    {
        if (id == 1) scoreTextLeft.UpdateAnimation();
        if (id == 2) scoreTextRight.UpdateAnimation();
    }

    private void EmitGoalParticles(byte id)
    {
        int value = Random.Range(10, 20);
        
        if (id == 1) goalParticlesPlayer1.EmitParticles(value);
        if (id == 2) goalParticlesPlayer2.EmitParticles(value);
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
