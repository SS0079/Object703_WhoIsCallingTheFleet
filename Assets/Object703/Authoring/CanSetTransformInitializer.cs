using KittyDOTS;
using Object703.Core;
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
                    AddComponent(self, new LocalPositionInitializer());
                }
                if (authoring.setRotation)
                {
                    AddComponent(self, new LocalRotationInitializer());
                }
                if (authoring.setScale)
                {
                    AddComponent(self, new LocalScaleInitializer());
                }
            }
        }
    }
}