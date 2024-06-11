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
        public NetCodeConfig netConfig;
        [FormerlySerializedAs("SelfDestructCountDown")]
        public float lifeSpan=0;
        public bool startCountDownImmediately = false;
    }
    public class CanBeDestructAuthoringBaker : Baker<CanBeDestructAuthoring>
    {
        public override void Bake(CanBeDestructAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            if (authoring.netConfig==null)
            {
                Debug.LogWarning($"Net code config missing!",authoring.gameObject);
                return;
            }
            AddComponent(self,new DestructTag());
            //if start count down immediately and life span >0, set self-destruct at tick to enabled
            //if life span is 0, simply set destruct tag enabled
            if (authoring.lifeSpan > 0)
            {
                var lifeSpanTick = (uint)(authoring.lifeSpan * authoring.netConfig.ClientServerTickRate.SimulationTickRate);
                AddComponent(self,new LifeSpanTick(){value = lifeSpanTick});
                if (authoring.startCountDownImmediately)
                {
                }
                else
                {
                
                }
                if (authoring.TryGetComponent(out GhostAuthoringComponent _))
                {
                    AddComponent(self,new SelfDestructAtTick());
                    SetComponentEnabled<SelfDestructAtTick>(self,false);
                }
            }
            else
            {
                SetComponentEnabled<DestructTag>(self,true);
            }
            
            
            SetComponentEnabled<DestructTag>(self,authoring.immediatelyDestruct);
        }
    }
    
    
}