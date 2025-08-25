using UnityEngine;

namespace Gameplay.Managers
{
    public abstract class BaseManager : MonoBehaviour
    {
        protected virtual void OnEnable() => SubscribeEvents();
        protected virtual void OnDisable() => UnsubscribeEvents();
    
        protected abstract void SubscribeEvents();
        protected abstract void UnsubscribeEvents();
    }
}