using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core
{
    // [GhostEnabledBit]
    public struct EndSpawnPrefabs : IComponentData
    {
        public Entity value0;
        public Entity value1;
        public Entity value2;
        public Entity value3;

        public void Spawn(EntityManager manager,LocalTransform trans,GhostOwner owner)
        {
            var newTrans = LocalTransform.FromPositionRotation(trans.Position,trans.Rotation);
            if (value0!=Entity.Null)
            {
                var e = manager.Instantiate(value0);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
            if (value1!=Entity.Null)
            {
                var e = manager.Instantiate(value1);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
            if (value2!=Entity.Null)
            {
                var e = manager.Instantiate(value2);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
            if (value3!=Entity.Null)
            {
                var e = manager.Instantiate(value3);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
        }
    }
    public struct CanEndSpawn : ICommandData
    {
        [GhostField]
        public NetworkTick Tick { get; set; }
        [GhostField]
        public InputEvent canSpawn;
    }

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnHitSystemGroup),OrderLast = true)]
    public partial struct EndSpawnSystem : ISystem
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
            foreach (var (hitSpawns,canEndSpawn,trans,owner,entity) in SystemAPI
                         .Query<RefRW<EndSpawnPrefabs>, DynamicBuffer<CanEndSpawn>,RefRO<LocalTransform>,RefRO<GhostOwner>>().WithAll<DestructTag>().WithEntityAccess())
            {
                canEndSpawn.GetDataAtTick(networkTime.ServerTick, out var cur);
                if (cur.canSpawn.IsSet)
                {
                    hitSpawns.ValueRW.Spawn(state.EntityManager,trans.ValueRO,owner.ValueRO);
                }
            }
        }
    }

    
}