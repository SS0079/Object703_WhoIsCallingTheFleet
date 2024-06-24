using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Core.Combat
{
    public struct HitSpawnBuffer : IBufferElementData
    {
        public Entity value;
    }

    public struct HitEffectBuffer : IBufferElementData
    {
        public Entity value;
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
            foreach (var (hitSpawns,ltw,entity) in SystemAPI
                         .Query<DynamicBuffer<HitSpawnBuffer>,RefRO<LocalTransform>>().WithAll<DestructTag>().WithNone<HideInClient>().WithEntityAccess())
            {
                for (int i = 0; i < hitSpawns.Length; i++)
                {
                    var e = state.EntityManager.Instantiate(hitSpawns[i].value);
                    Debug.Log($"{e} | {i} | {entity}");
                    SystemAPI.SetComponent(e,ltw.ValueRO);
                }
            }
        }
    }

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HitEffectSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // spawn all entity prefab stored in hit spawn buffer where destruct tag is on
            foreach (var (hitSpawns,ltw) in SystemAPI
                         .Query<DynamicBuffer<HitEffectBuffer>,RefRO<LocalTransform>>().WithAll<DestructTag,Simulate>().WithNone<HideInClient>())
            {
                //TODO: change this to spawn a game object
                for (int i = 0; i < hitSpawns.Length; i++)
                {
                    var e = state.EntityManager.Instantiate(hitSpawns[i].value);
                    SystemAPI.SetComponent(e,ltw.ValueRO);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}