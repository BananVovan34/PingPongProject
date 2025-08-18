using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Managers")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ParticlesManager particlesManager;
    [SerializeField] private PostFXManager postFXManager;
    [SerializeField] private CameraManager cameraManager;

    private Vector2 _initialPlayerPosition = new Vector2(10.35f, 0.0f);
    
    public Action OnReset;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }

    public void OnScoreZoneReached(byte playerId)
    {
        scoreManager.AddScore(playerId);

        particlesManager.EmitGoalParticles(playerId);
        postFXManager.PlayGoalEffects();
        cameraManager.DoShake(0.1f, 0.2f);

		OnReset?.Invoke();
    }
}
