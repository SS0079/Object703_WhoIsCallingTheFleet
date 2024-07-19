using KittyDOTS;
using Object703.Core;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanBeDestructAuthoring : MonoBehaviour
    {
        class CanBeDestructAuthoringBaker : Baker<CanBeDestructAuthoring>
        {
            public override void Bake(CanBeDestructAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                this.AddDisabledComponent(self, new DestructTag());
            }
        }
    }
}