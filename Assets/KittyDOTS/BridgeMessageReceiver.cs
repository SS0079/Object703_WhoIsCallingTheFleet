using Unity.Entities;
using UnityEngine;

namespace KittyDOTS
{
    [DisallowMultipleComponent]
    public class BridgeMessageReceiver : MonoBehaviour
    {
        
        class BridgeMessageReceiverBaker : Baker<BridgeMessageReceiver>
        {
            public override void Bake(BridgeMessageReceiver authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddBuffer<BridgeMessageBufferElement>(self);
            }
        }
    }
}