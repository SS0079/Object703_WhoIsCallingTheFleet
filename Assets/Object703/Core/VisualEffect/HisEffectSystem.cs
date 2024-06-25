using Object703.Core.Combat;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Object703.Core.VisualEffect
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
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
            foreach (var (hitSpawns,trans) in SystemAPI
                         .Query<DynamicBuffer<HitEffectBuffer>,RefRO<LocalTransform>>().WithAll<DestructTag,Simulate>().WithNone<HideInClient>())
            {
                //TODO: change this to spawn a game object
                for (int i = 0; i < hitSpawns.Length; i++)
                {
                    var e = state.EntityManager.Instantiate(hitSpawns[i].value);
                    SystemAPI.SetComponent(e, new LocalPositionInitializer() { position = trans.ValueRO.Position });
                }
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