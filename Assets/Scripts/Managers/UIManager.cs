using UnityEngine;

public class UIManager : BaseManager
{
    [Header("UI References")]
    [SerializeField] private ScoreText scoreTextLeft;
    [SerializeField] private ScoreText scoreTextRight;

    protected override void SubscribeEvents()
    {
        ScoreManager.OnScoreChanged += UpdateScoreUI;
    }

    protected override void UnsubscribeEvents()
    {
        ScoreManager.OnScoreChanged -= UpdateScoreUI;
    }
    
    private void UpdateScoreUI(int score1, int score2, byte playerId)
    {
        scoreTextLeft.SetScore(score1);
        scoreTextRight.SetScore(score2);
        
        if (playerId == 1) scoreTextLeft.UpdateAnimation();
        if (playerId == 2) scoreTextRight.UpdateAnimation();
    }
}