using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Gameplay.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(Animator))]
    public class PointsText : NetworkBehaviour
    {
        private static readonly int Update = Animator.StringToHash("update");

        private TextMeshProUGUI _pointsText;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _pointsText = GetComponent<TextMeshProUGUI>();
        }

        public void UpdateText(int value) =>
            UpdateTextClientRPC(value);
        
        [ClientRpc]
        private void UpdateTextClientRPC(int value) =>
            _pointsText.text = value.ToString();

        public void UpdateAnimation() =>
            UpdateAnimationClientRPC();
        
        [ClientRpc]
        private void UpdateAnimationClientRPC() =>
            _animator.SetTrigger(Update);
    }
}