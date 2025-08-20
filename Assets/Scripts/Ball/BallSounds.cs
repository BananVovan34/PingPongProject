using UnityEngine;

public class BallSounds : MonoBehaviour
{
    [SerializeField] private AudioSource ballHitSound;

    private void PlayBallHitSound(Vector2 obj) => ballHitSound.Play();
    
    private void OnEnable()
    {
        BallEvents.OnBallHitPaddle += PlayBallHitSound;
        BallEvents.OnBallHitWall += PlayBallHitSound;
    }
    
    private void OnDisable() 
    {
        BallEvents.OnBallHitPaddle -= PlayBallHitSound;
        BallEvents.OnBallHitWall -= PlayBallHitSound;
    }
}
