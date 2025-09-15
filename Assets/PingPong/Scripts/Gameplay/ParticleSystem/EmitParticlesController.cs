using UnityEngine;

namespace Gameplay.ParticleSystem
{
    [RequireComponent(typeof(UnityEngine.ParticleSystem))]
    public class EmitParticlesController : MonoBehaviour
    {
        private UnityEngine.ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<UnityEngine.ParticleSystem>();
            if (_particleSystem == null)
            {
                throw new System.NullReferenceException("Particle system is null");
            }
        }

        public void EmitParticles(int amount)
        {
            _particleSystem.Emit(amount);
        }
    }
}
