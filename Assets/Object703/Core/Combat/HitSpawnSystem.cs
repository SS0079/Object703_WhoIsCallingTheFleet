using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Core.Combat
{
    public struct HitSpawnBufferElement : IBufferElementData
    {
        public Entity value;
    }

    public struct HitEffectBufferElement : IBufferElementData
    {
        public Entity value;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct HitSpawnServerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // spawn all entity prefab stored in hit spawn buffer where destruct tag is on
            foreach (var (hitSpawns,ltw) in SystemAPI.Query<DynamicBuffer<HitSpawnBufferElement>,RefRO<LocalTransform>>().WithAll<DestructTag>())
            {
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

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HitSpawnClientSystem : ISystem
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
                         .Query<DynamicBuffer<HitEffectBufferElement>,RefRO<LocalTransform>>().WithAll<DestructTag>())
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