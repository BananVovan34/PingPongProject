using UnityEngine;

public class EmitParticlesController : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
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
