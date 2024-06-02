using Object703.Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Core.VisualEffect
{
    [DisallowMultipleComponent]
    public class ScaleProxyAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("Scale")]
        public LocalTransformScaleProxy scale;
    }
    public class ScaleProxyAuthoringBaker : Baker<ScaleProxyAuthoring>
    {
        public override void Bake(ScaleProxyAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(self,authoring.scale);
        }
    }
}