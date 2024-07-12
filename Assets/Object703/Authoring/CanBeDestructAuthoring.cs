using KittyDOTS;
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
                this.AddEnableComponent(self, new DestructTag(), authoring.destructImmediately);
            }
        }
    }
}