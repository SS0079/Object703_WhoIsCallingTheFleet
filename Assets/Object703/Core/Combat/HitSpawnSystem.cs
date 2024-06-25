using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Core.Combat
{
    public struct HitSpawnBuffer : IBufferElementData
    {
        public Entity value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct HitEffectBuffer : IBufferElementData
    {
        public Entity value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct LocalPositionInitializer : IComponentData , IEnableableComponent
    {
        public float3 position;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct LocalRotationInitializer : IComponentData , IEnableableComponent
    {
        public quaternion rotation;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct LocalScaleInitializer : IComponentData , IEnableableComponent
    {
        public float3 scale;
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
                    SystemAPI.SetComponent(e,ltw.ValueRO);
                }
            }
        }
    }

    
}