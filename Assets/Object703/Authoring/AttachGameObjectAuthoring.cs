using System;
using Object703.Core.VisualEffect;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Object703.Authoring
{
    
    [DisallowMultipleComponent]
    public class AttachGameObjectAuthoring : MonoBehaviour
    {
        public string prefabName;
        class GameObjectActorAuthoringBaker : Baker<AttachGameObjectAuthoring>
        {
            public override void Bake(AttachGameObjectAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponent(self,new AttachGameObject(){prefabName = authoring.prefabName});
            }
        }
    }
}