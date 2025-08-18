using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    private static readonly int Update = Animator.StringToHash("update");
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Animator animator;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void UpdateAnimation()
    {
        animator.SetTrigger(Update);
    }
}
