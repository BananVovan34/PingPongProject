using Unity.Netcode;
using UnityEngine;

namespace PingPong.Scripts.Core.Network
{
    [System.Serializable]
    public class MovementData : INetworkSerializable
    {
        public int tick;
        public Vector2 direction;
        public Vector2 velocity;
        public Vector2 position;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref direction);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref position);
        }
    }
}