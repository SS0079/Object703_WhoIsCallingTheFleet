using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace KittyDOTS
{
    public static class BridgeMessageUtility
    {
        public static bool SendMessageBuffer(BridgeMessageBufferElement msg)
        {
            EntityQuery bridgeQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BridgeMessageBufferElement>()
                .Build(World.DefaultGameObjectInjectionWorld.EntityManager);
            var result = false;

            if (bridgeQuery.TryGetSingletonBuffer(out DynamicBuffer<BridgeMessageBufferElement> buffer))
            {
                buffer.Add(msg);
                result=true;
            }
            bridgeQuery.Dispose();
            return result;
        }
    }
    
    [Serializable]
    public struct BridgeMessageBufferElement : IBufferElementData
    {
        public BridgeMessage message;
        public float4 floatParams;
        public int4 intParams;
    }

    public enum BridgeMessage
    {
        PlayerShipMove,
        PlayerShipRotate,
    }
}