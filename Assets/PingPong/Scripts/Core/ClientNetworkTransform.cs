using Unity.Netcode.Components;

namespace PingPong.Scripts.Core
{
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}