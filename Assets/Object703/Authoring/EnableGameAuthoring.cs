using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    public struct EnableGameTag : IComponentData
    {
    }
    [DisallowMultipleComponent]
    public class EnableGameAuthoring : MonoBehaviour
    {
        
        class EnableGameAuthoringBaker : Baker<EnableGameAuthoring>
        {
            public override void Bake(EnableGameAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponent(self,new EnableGameTag());
            }
        }
    }
}