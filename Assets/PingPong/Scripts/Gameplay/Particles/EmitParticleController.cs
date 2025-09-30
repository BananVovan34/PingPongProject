using System;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.Particles
{
    [RequireComponent(typeof(ParticleSystem))]
    public class EmitParticleController : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        private void Awake() =>
            _particleSystem = GetComponent<ParticleSystem>();
        
        public void Emit(int value) =>
            _particleSystem.Emit(value);

        public void EmitRandomValue(int minValue, int maxValue)
        {
            int value = UnityEngine.Random.Range(minValue, maxValue);
            
            _particleSystem.Emit(value);
        }
    }
}