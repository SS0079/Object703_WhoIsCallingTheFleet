using Object703.Core.NetCode;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class IsSubControllerAuthoring : MonoBehaviour
    {
        
        class IsSubControllerBaker : Baker<IsSubControllerAuthoring>
        {
            public override void Bake(IsSubControllerAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponent(self,new GhostParent());
                AddComponent(self,new Parent());
            }
        }
    }
}