using Unity.Netcode;

namespace Gameplay.Managers
{
    public abstract class BaseNetworkGameManager : NetworkBehaviour
    {
        protected virtual void OnEnable() => SubscribeEvents();
        protected virtual void OnDisable() => UnsubscribeEvents();
    
        protected abstract void SubscribeEvents();
        protected abstract void UnsubscribeEvents();
    }
}