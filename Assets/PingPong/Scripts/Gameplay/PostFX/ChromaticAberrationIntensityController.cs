using PingPong.Scripts.Gameplay.Ball;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PingPong.Scripts.Gameplay.PostFX
{
    public class ChromaticAberrationIntensityController : BasePostFXController
    {
        private Volume _volume;
        private ChromaticAberration _chromaticAberration;
        private float _defaultIntensity;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            if (_volume == null)
                throw new System.NullReferenceException("Volume is null");

            if (!_volume.profile.TryGet(out _chromaticAberration))
                throw new System.NullReferenceException("Bloom profile is null");

            _defaultIntensity = _chromaticAberration.intensity.value;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            BallController.Instance.OnScoreZoneReached += TriggerEffect;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            BallController.Instance.OnScoreZoneReached -= TriggerEffect;
        }

        private void TriggerEffect(string obj) =>
            TriggerEffectClientRPC();

        protected override void ApplyEffect(float curveValue)
        {
            _chromaticAberration.intensity.value = _defaultIntensity + curveValue;
        }

        protected override void ResetToDefault()
        {
            _chromaticAberration.intensity.value = _defaultIntensity;
        }
    }
}