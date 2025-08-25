using System.Collections;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraShake : MonoBehaviour
    {
        private Vector3 _initialPosition;

        private void Awake()
        {
            _initialPosition = transform.position;
        }

        public void StartShake(float offset, float duration)
        {
            StopShake();
            StartCoroutine(ShakeSequence(offset, duration));
        }

        public void StopShake()
        {
            StopAllCoroutines();
            transform.localPosition = _initialPosition;
        }

        private IEnumerator ShakeSequence(float offset, float duration)
        {
            float durationPassed = 0f;

            while (durationPassed < duration)
            {
                Shake(offset);
                durationPassed += Time.deltaTime;
                yield return null;
            }
        
            transform.localPosition = _initialPosition;
        }

        private void Shake(float maxOffset)
        {
            float xOffset = Random.Range(-maxOffset, maxOffset);
            float yOffset = Random.Range(-maxOffset, maxOffset);
            transform.localPosition = _initialPosition + new Vector3(xOffset, yOffset, 0);
        }
    }
}
