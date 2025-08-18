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
    
    public void OnScoreZoneReached(byte playerId)
    {
        RoundEvents.OnRoundEnd(playerId);
        RoundEvents.OnRoundStart();
    }

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }
    
    private void OnEnable()
    {
        BallEvents.OnBallScored += OnScoreZoneReached;
        RoundEvents.RoundStart += ResetRound;
    }

    private void OnDisable()
    {
        BallEvents.OnBallScored -= OnScoreZoneReached;
        RoundEvents.RoundStart -= ResetRound;
    }
    
    private void ResetRound() { OnReset?.Invoke(); }
}
