using UnityEngine;

namespace Gameplay.Ball
{
    [RequireComponent(typeof(TrailRenderer))]
    public class BallTrail : MonoBehaviour
    {
        private TrailRenderer trailRenderer;

        private void Awake()
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                throw new System.NullReferenceException("TrailRenderer is null");
            }
        }
    
        private void OnEnable()
        {
            BallEvents.OnBallLaunch += EnableTrail;
            BallEvents.OnBallReset += DisableTrail;
        }
    
        private void OnDisable() 
        {
            BallEvents.OnBallLaunch -= EnableTrail;
            BallEvents.OnBallReset -= DisableTrail;
        }

        private void EnableTrail() => trailRenderer.enabled = true;
        private void DisableTrail() => trailRenderer.enabled = false;
    }
}