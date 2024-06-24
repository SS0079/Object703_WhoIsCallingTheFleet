using System;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Serialization;

namespace Object703.Core.VisualEffect
{
    [Serializable]
    public struct LocalTransformScaleProxy : IComponentData
    {
        [FormerlySerializedAs("Value")]
        public float value;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct ScaleProxySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            new SyncScaleJob().Run();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        [WithNone(typeof(HideInClient))]
        public partial struct SyncScaleJob : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                ref LocalTransform localTransform,
                in LocalTransformScaleProxy proxy)
            {
                localTransform.Scale = proxy.value;
            }
        }
    }
}