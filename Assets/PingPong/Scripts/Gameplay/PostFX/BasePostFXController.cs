using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.PostFX
{
    public abstract class BasePostFXController : NetworkBehaviour
    {
        [Header("FX Settings")]
        [SerializeField] protected AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1), new Keyframe(0, 0));
        [SerializeField] protected float effectDuration = 0.35f;

        private float _timer;
        private bool _isActive;

        private void Update()
        {
            PostFXHandler();
        }

        /// <summary>
        /// Curve processing and applying the value to the effect
        /// </summary>
        private void PostFXHandler()
        {
            if (!_isActive) return;

            _timer -= Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(1 - (_timer / effectDuration));
            float curveValue = curve.Evaluate(normalizedTime);

            ApplyEffect(curveValue);

            if (_timer <= 0f)
                ResetEffect();
        }

        protected abstract void ApplyEffect(float curveValue);
        protected abstract void ResetToDefault();

        [ClientRpc]
        public void TriggerEffectClientRPC() => TriggerEffect();
        
        public void TriggerEffect() => TriggerEffect(effectDuration);

        public void TriggerEffect(float duration)
        {
            _timer = duration;
            _isActive = true;
        }

        public void StopEffect() => ResetEffect();

        private void ResetEffect()
        {
            _timer = 0f;
            _isActive = false;
            ResetToDefault();
        }
    }
}