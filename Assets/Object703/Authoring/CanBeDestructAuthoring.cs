using Object703.Core.Recycle;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring.Installer
{
    [DisallowMultipleComponent]
    public class CanBeDestructAuthoring : MonoBehaviour
    {
        public bool destructImmediately;
        class CanBeDestructAuthoringBaker : Baker<CanBeDestructAuthoring>
        {
            public override void Bake(CanBeDestructAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponent(self,new DestructTag());
                SetComponentEnabled<DestructTag>(self,authoring.destructImmediately);
            }
        }
    }
}