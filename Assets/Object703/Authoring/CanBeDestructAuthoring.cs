using KittyDOTS;
using Object703.Core;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanBeDestructAuthoring : MonoBehaviour
    {
        public bool destrcutNextFrame;
        class CanBeDestructAuthoringBaker : Baker<CanBeDestructAuthoring>
        {
            public override void Bake(CanBeDestructAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                this.AddEnableComponent(self, new DestructTag(), false);
                if (authoring.destrcutNextFrame)
                {
                    AddComponent(self,new DestructNextFrameTag());
                }
            }
        }
    }
}