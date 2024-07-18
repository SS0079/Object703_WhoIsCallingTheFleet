using Object703.Core;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanFallowByCameraAuthoring : MonoBehaviour
    {
    }
    public class CanFallowByCameraBaker : Baker<CanFallowByCameraAuthoring>
    {
        public override void Bake(CanFallowByCameraAuthoring targetAuthoring)
        {
            var self = GetEntity(TransformUsageFlags.None);
            AddComponent(self,new NewCameraTargetTag());
        }
    }
}