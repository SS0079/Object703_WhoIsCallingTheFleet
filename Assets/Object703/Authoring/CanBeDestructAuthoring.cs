using System;
using Object703.Core.Recycle;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanBeDestructAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("SelfDestructCountDown")]
        public float selfDestructCountDown=0;
        [FormerlySerializedAs("ImmediatelyDestruct")]
        public bool immediatelyDestruct = false;
    }
    public class CanBeDestructAuthoringBaker : Baker<CanBeDestructAuthoring>
    {
        public override void Bake(CanBeDestructAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(self,new DestructTag());
            SetComponentEnabled<DestructTag>(self,authoring.immediatelyDestruct);
            if (authoring.selfDestructCountDown > 0)
            {
                AddComponent(self,new SelfDestructTimer(){value = authoring.selfDestructCountDown});
                if (authoring.TryGetComponent(out GhostAuthoringComponent _))
                {
                    AddComponent(self,new SelfDestructAtTick());
                    SetComponentEnabled<SelfDestructAtTick>(self,false);
                }
            }
        }
    }
    
    
}