using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gameplay.PostFX
{
    [RequireComponent(typeof(Volume))]
    public class BloomIntensityController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve additionalIntensityValueCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField] private float effectDuration = 0.35f;
        [SerializeField] private bool debug = false;
    
        private Volume _volume;
        private Bloom _bloom;
        private float _defaultBloomIntensity;
        private float _timer;
        private bool _isActive;

        private void Awake()
        {
            _volume  = GetComponent<Volume>();
            if (_volume == null)
            {
                throw new System.NullReferenceException("Volume is null");
            }

            if (!_volume.profile.TryGet(out _bloom))
            {
                throw new System.NullReferenceException("Bloom profile is null");
            }

            _defaultBloomIntensity = _bloom.intensity.value;
        }

        private void Update()
        {
            if (!_isActive) return;
        
            _timer -= Time.deltaTime;
            float normalizedTime = 1 - (_timer / effectDuration);
            float curveValue = additionalIntensityValueCurve.Evaluate(normalizedTime);
            _bloom.intensity.value = _defaultBloomIntensity + curveValue;
        
            if (debug) Debug.Log(normalizedTime + " | " + _bloom.intensity.value);

            if (_timer <= 0f)
                ResetEffect();
        }

        public void TriggerEffect() { TriggerEffect(effectDuration); }

        public void TriggerEffect(float duration)
        {
            _timer = duration;
            _isActive = true;
        }

        private void ResetEffect()
        {
            _timer = 0f;
            _bloom.intensity.value = _defaultBloomIntensity;
            _isActive = false;
        }
    
        public void StopEffect() { ResetEffect(); }
    }
}
