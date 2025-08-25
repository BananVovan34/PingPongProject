using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followStrenth = 0.1f;
        [SerializeField] private float maxOffset = 0.5f;
    
        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;
        }

        private void Update()
        {
            if (target == null) return;
        
            Vector3 offset = (target.position - initialPosition) * followStrenth;
        
            offset = Vector3.ClampMagnitude(offset, maxOffset);
        
            transform.position = Vector3.Lerp(transform.position, initialPosition + offset, Time.deltaTime * 5f);
        }
    }
}
