using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    [Header("Particles References")]
    [SerializeField] private EmitParticlesController goalParticlesPlayer1;
    [SerializeField] private EmitParticlesController goalParticlesPlayer2;
    
    public void EmitGoalParticles(byte id)
    {
        int value = Random.Range(10, 20);
        
        if (id == 1) goalParticlesPlayer1.EmitParticles(value);
        if (id == 2) goalParticlesPlayer2.EmitParticles(value);
    }
}