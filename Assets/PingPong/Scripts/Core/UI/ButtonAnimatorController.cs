using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace PingPong.Scripts.Core.UI
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Animator))]
    public class ButtonAnimatorController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _animator.SetTrigger("OnHover");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _animator.SetTrigger("OnExit");
        }
    }
}