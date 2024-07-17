using System;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Serialization;

namespace Object703.Core.OnHit
{
    [Serializable]
    public struct Hp : IComponentData
    {
        [FormerlySerializedAs("Max")]
        public float max;
        [FormerlySerializedAs("Current")]
        [GhostField]public float current;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted,OwnerSendType = SendToOwnerType.SendToNonOwner)]
    public struct DamageThisTick : ICommandData
    {
        [GhostField] public NetworkTick Tick { get; set; }
        [GhostField] public float value;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnHitSystemGroup))]
    [UpdateAfter(typeof(DamageBufferSystem))]
    public partial struct DamageApplySystem : ISystem
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
            var currentTick = networkTime.ServerTick;
            foreach (var (damageBuffer,damageThisTick) in SystemAPI
                         .Query<DynamicBuffer<DamageBuffer>,DynamicBuffer<DamageThisTick>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                if (damageBuffer.IsEmpty)
                {
                    damageThisTick.AddCommandData(new DamageThisTick { Tick = currentTick, value = 0 });
                }
                else
                {
                    var totalDamage = 0f;
                    if (damageThisTick.GetDataAtTick(currentTick, out var damageTick))
                    {
                        totalDamage = damageTick.value;
                    }

                    foreach (var damage in damageBuffer)
                    {
                        totalDamage += damage.value;
                    }

                    damageThisTick.AddCommandData(new DamageThisTick { Tick = currentTick, value = totalDamage });
                    damageBuffer.Clear();
                }
            }
            
            foreach (var (hp, damageThisTickBuffer, enDestructTag) in SystemAPI
                         .Query<RefRW<Hp>, DynamicBuffer<DamageThisTick>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                if(!damageThisTickBuffer.GetDataAtTick(currentTick, out var damageThisTick)) continue;
                if (damageThisTick.Tick != currentTick) continue;
                hp.ValueRW.current -= damageThisTick.value;

                if (hp.ValueRO.current <= 0)
                {
                    enDestructTag.ValueRW = true;
                }
            }
        }

    }
}