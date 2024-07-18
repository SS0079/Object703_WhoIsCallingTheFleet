using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Object703.Core
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(BeforeDestructSystemGroup))]
    public partial struct InitializeSelfDestructSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            //prepare SelfDestructAtTick
            foreach (var (lifeSpan,destructAtTick,prepared) in SystemAPI
                         .Query<RefRO<LifeSpanTick>
                             ,DynamicBuffer<SelfDestructAtTick>
                             ,EnabledRefRW<SelfDestructPrepared>>().WithAll<Simulate>().WithNone<DestructTag>().WithDisabled<SelfDestructPrepared>())
            {
                var localTick = currentTick.AddSpan(lifeSpan.ValueRO.value);
                destructAtTick.AddCommandData(new SelfDestructAtTick
                {
                    Tick = currentTick,
                    value = localTick
                });
                prepared.ValueRW = true;
            }
        }
    }
}