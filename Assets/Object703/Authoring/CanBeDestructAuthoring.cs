using KittyDOTS;
using Object703.Core;
using Unity.Entities;
using Unity.NetCode;
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
                if (authoring.TryGetComponent(out GhostAuthoringComponent _))
                {
                    this.AddDisabledComponent(self, new HideInClient());
                }
            }
        }
    }
}