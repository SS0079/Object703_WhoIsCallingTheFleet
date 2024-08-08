using Object703.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class LineRendererActorAuthoring : MonoBehaviour
    {
        public string prefabName;
        class LineRendererActorAuthoringBaker : Baker<LineRendererActorAuthoring>
        {
            public override void Bake(LineRendererActorAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponentObject(self,new AttachLineRenderer(){prefabName = authoring.prefabName});
            }
        }
    }
}