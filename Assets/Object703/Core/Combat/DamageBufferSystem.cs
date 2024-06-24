using System;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.Serialization;

namespace Object703.Core.Combat
{
    [Serializable]
    public struct DamageBuffer : IBufferElementData
    {
        public float value;
    }
    
    [Serializable]
    public struct DealDamage : IComponentData
    {
        [FormerlySerializedAs("Value")]
        public float value;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HitCheckSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct DamageBufferSystem : ISystem
    {
        // private ComponentLookup<DestructTag> destructTagLp;
        private BufferLookup<DamageBuffer> damageBufferLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // destructTagLp = SystemAPI.GetComponentLookup<DestructTag>(false);
            damageBufferLp = SystemAPI.GetBufferLookup<DamageBuffer>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // destructTagLp.Update(ref state);
            damageBufferLp.Update(ref state);
            state.Dependency.Complete();
            foreach (var (hitBuffer,damage) in SystemAPI
                         .Query<DynamicBuffer<HitCheckResult>,DealDamage>().WithAll<Simulate>().WithNone<HideInClient>())
            {
                for (int i = 0; i < hitBuffer.Length; i++)
                {
                    var target = hitBuffer[i].target;
                    if(!damageBufferLp.HasBuffer(target)) continue;
                    damageBufferLp[target].Add(new DamageBuffer() { value = damage.value });
                }
            }
        }
    }
}