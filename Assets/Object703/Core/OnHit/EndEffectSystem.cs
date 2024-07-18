using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core
{
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct EndEffectPrefabs : IComponentData , IEnableableComponent
    {
        public Entity value0;
        public Entity value1;
        public Entity value2;
        public Entity value3;
        
        public void Spawn(EntityManager manager,LocalTransform trans)
        {
            if (value0!=Entity.Null)
            {
                var e = manager.Instantiate(value0);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
            if (value1!=Entity.Null)
            {
                var e = manager.Instantiate(value1);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
            if (value2!=Entity.Null)
            {
                var e = manager.Instantiate(value2);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
            if (value3!=Entity.Null)
            {
                var e = manager.Instantiate(value3);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
        }
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
    [UpdateInGroup(typeof(AfterHitSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct EndEffectSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // spawn all entity prefab stored in hit spawn buffer where destruct tag is on
            foreach (var (hitSpawns,enHitSpawn,trans) in SystemAPI
                         .Query<RefRW<EndEffectPrefabs>,EnabledRefRW<EndEffectPrefabs>,RefRO<LocalTransform>>().WithAll<DestructTag,Simulate>())
            {
                //TODO: change this to spawn a game object
                hitSpawns.ValueRW.Spawn(state.EntityManager,trans.ValueRO);
                enHitSpawn.ValueRW = false;
            }

            //sync position for new spawn effect
            foreach (var (positionProxy,trans) in SystemAPI
                         .Query<RefRO<LocalPositionInitializer>,RefRW<LocalTransform>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                trans.ValueRW.Position = positionProxy.ValueRO.position;
            }

            //disable all local position proxy, those should have been handled
            foreach (var enPosProxy in SystemAPI
                         .Query<EnabledRefRW<LocalPositionInitializer>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                enPosProxy.ValueRW = false;
            }
        }
    }
}