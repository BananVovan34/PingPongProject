using UnityEngine;

namespace Gameplay.ScoreZone
{
    public class ScoreZoneController : MonoBehaviour
    {
        [SerializeField] [Range(1, 2)] private byte id;

        public byte ID => id;
    }
}
