using Object703.Core;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class LineRendererActorAuthoring : MonoBehaviour
    {
        public LineRenderer prefab;
        class LineRendererActorAuthoringBaker : Baker<LineRendererActorAuthoring>
        {
            public override void Bake(LineRendererActorAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponentObject(self,new AttachLineRenderer(){prefab = authoring.prefab.gameObject});
            }
        }
    }
}