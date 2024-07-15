using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Core.Combat
{
    public struct HitSpawnPrefabs : IComponentData , IEnableableComponent
    {
        public Entity value0;
        public Entity value1;
        public Entity value2;
        public Entity value3;

        public void Spawn(EntityManager manager,LocalTransform trans)
        {
            var newTrans = LocalTransform.FromPositionRotation(trans.Position,trans.Rotation);
            if (value0!=Entity.Null)
            {
                var e = manager.Instantiate(value0);
                manager.SetComponentData(e,newTrans);
            }
            if (value1!=Entity.Null)
            {
                var e = manager.Instantiate(value1);
                manager.SetComponentData(e,newTrans);
            }
            if (value2!=Entity.Null)
            {
                var e = manager.Instantiate(value2);
                manager.SetComponentData(e,newTrans);
            }
            if (value3!=Entity.Null)
            {
                var e = manager.Instantiate(value3);
                manager.SetComponentData(e,newTrans);
            }
        }
    }

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct HitSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if(!networkTime.IsFirstTimeFullyPredictingTick) return;
            
            // spawn all entity prefab stored in hit spawn buffer where destruct tag is on
            foreach (var (hitSpawns,enHitSpawns,trans,entity) in SystemAPI
                         .Query<RefRW<HitSpawnPrefabs>, EnabledRefRW<HitSpawnPrefabs>,RefRO<LocalTransform>>().WithAll<DestructTag>().WithEntityAccess())
            {
                hitSpawns.ValueRW.Spawn(state.EntityManager,trans.ValueRO);
                enHitSpawns.ValueRW = false;
            }
        }
    }

    
}