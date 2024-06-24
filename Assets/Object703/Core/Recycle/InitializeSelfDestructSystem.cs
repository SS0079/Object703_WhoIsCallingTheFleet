using Object703.Core.NetCode;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.NetCode;

namespace Object703.Core.Recycle
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
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
                             ,EnabledRefRW<SelfDestructPrepared>>().WithAll<Simulate>().WithDisabled<SelfDestructPrepared>())
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

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}