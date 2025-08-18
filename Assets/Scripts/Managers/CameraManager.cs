using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    [Header("Camera References")]
    [SerializeField] private CameraShake cameraShake;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }
    
    public void DoShake(float offset, float duration)
    {
        cameraShake.StartShake(offset, duration);
    }
}