using KittyDOTS;
using Object703.Core.Combat;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanSetTransformInitializer : MonoBehaviour
    {
        public bool setPosition;
        public bool setRotation;
        public bool setScale;
        class CanSetTransformProxyBaker : Baker<CanSetTransformInitializer>
        {
            public override void Bake(CanSetTransformInitializer authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                if (authoring.setPosition)
                {
                    this.AddEnableComponent(self, new LocalPositionInitializer());
                }
                if (authoring.setRotation)
                {
                    this.AddEnableComponent(self, new LocalRotationInitializer());
                }
                if (authoring.setScale)
                {
                    this.AddEnableComponent(self, new LocalScaleInitializer());
                }
            }
        }
    }
}