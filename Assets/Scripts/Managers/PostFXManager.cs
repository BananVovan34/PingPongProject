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
    
    public void PlayGoalEffects()
    {
        bloomIntensityController.TriggerEffect();
        chromaticAberrationController.TriggerEffect();
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