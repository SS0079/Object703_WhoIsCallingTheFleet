using System;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core
{
    [Serializable]
    public struct PredicateRange : IComponentData
    {
        public float range;
        public float margin;
        public float inRangeSq => range * range;
        public float outRangeSq => (range + margin)*(range+margin);
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PredicateSwitchSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PredicateRange>();
            state.RequireForUpdate<GhostPredictionSwitchingQueues>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var predicateRange = SystemAPI.GetSingleton<PredicateRange>();
            var inRangeSq = predicateRange.inRangeSq;
            var outRangeSq = predicateRange.outRangeSq;
            var predicaterEnt = SystemAPI.GetSingletonEntity<PredicateRange>();
            var predicaterPos = state.EntityManager.GetComponentData<LocalTransform>(predicaterEnt).Position;
            var switchQueue = SystemAPI.GetSingletonRW<GhostPredictionSwitchingQueues>().ValueRW;

            foreach (var (trans,e) in SystemAPI
                         .Query<RefRO<LocalTransform>>().WithAll<Simulate,GhostInstance>().WithNone<PredictedGhost,DestructTag,GhostOwnerIsLocal>().WithEntityAccess())
            {
                var distancesq = math.distancesq(trans.ValueRO.Position,predicaterPos);
                if (distancesq<=inRangeSq)
                {
                    switchQueue.ConvertToPredictedQueue.Enqueue(new ConvertPredictionEntry
                    {
                        TargetEntity = e,
                        TransitionDurationSeconds = 1
                    });
                }
            }
            foreach (var (trans,e) in SystemAPI
                         .Query<RefRO<LocalTransform>>().WithAll<Simulate,PredictedGhost,GhostInstance>().WithNone<DestructTag,GhostOwnerIsLocal>().WithEntityAccess())
            {
                var distancesq = math.distancesq(trans.ValueRO.Position,predicaterPos);
                if (distancesq>outRangeSq)
                {
                    switchQueue.ConvertToInterpolatedQueue.Enqueue(new ConvertPredictionEntry
                    {
                        TargetEntity = e,
                        TransitionDurationSeconds = 1
                    });
                }
            }
        }

    }
}