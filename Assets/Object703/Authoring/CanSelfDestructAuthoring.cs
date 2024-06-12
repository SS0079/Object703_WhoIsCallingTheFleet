using System;
using KittyDOTS;
using Object703.Authoring.Installer;
using Object703.Core.Recycle;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanBeDestructAuthoring))]
    public class CanSelfDestructAuthoring : MonoBehaviour
    {
        public NetCodeConfig netConfig;
        public float lifeSpan=0;
        public bool startCountDownImmediately = true;
    }
    public class CanBeDestructAuthoringBaker : Baker<CanSelfDestructAuthoring>
    {
        public override void Bake(CanSelfDestructAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            
            //if this is a ghost, consider add LifeSpanTick and SelfDestructAtTick etc.
            //else, add LifeSpanSecond
            
            //if life span >0 and startCountDownImmediately == true, add selfDestructAtTick and set it disabled
            //DestructSystem will catch this and set selfDestructAtTick than enable it
            
            //if life span >0 and startCountDownImmediately == false, do not add selfDestructAtTick
            //it should be added when count down start at runtime
            
            //if life span == 0 and startCountDownImmediately == true, just add SelfDestructAtTick with enabled and set the tick 0
            //DestructSystem will catch this and immediately set DestructTag enabled
            
            //if life span==0 and startCountDownImmediately==false,do nothing and throw a warning, cause this is meaningless


            if (authoring.TryGetComponent(out GhostAuthoringComponent _))
            {
                if (authoring.netConfig==null)
                {
                    Debug.LogWarning($"Net code config missing!",authoring.gameObject);
                    return;
                }
                if (authoring.lifeSpan > 0)
                {
                    var lifeSpanTick = (uint)(authoring.lifeSpan * authoring.netConfig.ClientServerTickRate.SimulationTickRate);
                    AddComponent(self,new LifeSpanTick(){value = lifeSpanTick});
                    if (authoring.startCountDownImmediately)
                    {
                        AddBuffer<SelfDestructAtTick>(self);
                        this.AddEnableComponent(self, new SelfDestructPrepared(), false);
                    }
                }
                else
                {
                    if (authoring.startCountDownImmediately)
                    {
                        this.AddEnableComponent(self, new SelfDestructPrepared());
                        AddBuffer<SelfDestructAtTick>(self);
                        AppendToBuffer(self,new SelfDestructAtTick
                        {
                            Tick = new NetworkTick(0),
                            value = new NetworkTick(0)
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"Immediately count down with a 0 lifespan is meaningless",authoring.gameObject);
                    }
                }
            }
            else
            {
                this.AddEnableComponent(self, new LifeSpanSecond() { value = authoring.lifeSpan }, authoring.startCountDownImmediately);
            }
            
            
        }
    }
    
    
}