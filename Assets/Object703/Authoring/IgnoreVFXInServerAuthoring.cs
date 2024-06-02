using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Object703.Authoring
{
    [GhostComponent(PrefabType = GhostPrefabType.Server)]
    public struct RemoveVFXTag : IComponentData
    {
        
    }
    [DisallowMultipleComponent]
    public class IgnoreVFXInServerAuthoring : MonoBehaviour
    {
        
        class IgnoreVFXInServerBaker : Baker<IgnoreVFXInServerAuthoring>
        {
            public override void Bake(IgnoreVFXInServerAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponent(self, ComponentType.ReadOnly<RemoveVFXTag>());
            }
        }
    }
    
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct RemoveVFXServerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var particlePrefabs = SystemAPI.QueryBuilder().WithAll<ParticleSystem, ParticleSystemRenderer, RemoveVFXTag,Prefab>().Build().ToEntityArray(Allocator.Temp);
            state.EntityManager.RemoveComponent<ParticleSystem>(particlePrefabs);
            state.EntityManager.RemoveComponent<ParticleSystemRenderer>(particlePrefabs);
            var particle = SystemAPI.QueryBuilder().WithAll<ParticleSystem, ParticleSystemRenderer, RemoveVFXTag>().Build().ToEntityArray(Allocator.Temp);
            state.EntityManager.RemoveComponent<ParticleSystem>(particle);
            state.EntityManager.RemoveComponent<ParticleSystemRenderer>(particle);
        }
    }
}