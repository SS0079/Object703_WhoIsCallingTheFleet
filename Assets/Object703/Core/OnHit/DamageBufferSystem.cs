using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Serialization;

namespace Object703.Core
{
    [Serializable]
    public struct DamageBuffer : IBufferElementData
    {
        public float value;
    }
    [Serializable]
    public struct AlreadyDamaged : IBufferElementData
    {
        public Entity value;
    }
    [Serializable]
    public struct DealDamage : IComponentData
    {
        [FormerlySerializedAs("Value")]
        public float value;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnHitSystemGroup))]
    [UpdateAfter(typeof(HitCheckSystem))]
    public partial struct DamageBufferSystem : ISystem
    {
        private BufferLookup<DamageBuffer> damageBufferLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            damageBufferLp = SystemAPI.GetBufferLookup<DamageBuffer>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            damageBufferLp.Update(ref state);
            state.Dependency.Complete();
            var alreadyDamagedHash = new NativeHashSet<Entity>(5,Allocator.Temp);

            // add damage to damage buffer
            foreach (var (hitBuffer,alreadyDamaged,damage) in SystemAPI
                         .Query<DynamicBuffer<HitCheckResult>,DynamicBuffer<AlreadyDamaged>, RefRO<DealDamage>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                alreadyDamagedHash.Clear();
                for (int i = 0; i < alreadyDamaged.Length; i++)
                {
                    var item = alreadyDamaged[i].value;
                    alreadyDamagedHash.Add(item);
                }
                for (int i = 0; i < hitBuffer.Length; i++)
                {
                    var target = hitBuffer[i].target;
                    
                    if(!damageBufferLp.HasBuffer(target) || alreadyDamagedHash.Contains(target)) continue;
                    damageBufferLp[target].Add(new DamageBuffer() { value = damage.ValueRO.value });
                    alreadyDamaged.Add(new AlreadyDamaged { value = target });
                }
            }
        }
    }
}