using System;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine.Serialization;

namespace Object703.Core.Combat
{
    [Serializable]
    public struct Hp : IComponentData
    {
        [FormerlySerializedAs("Max")]
        public float max;
        [FormerlySerializedAs("Current")]
        [GhostField]public float current;
    }
    
    [Serializable]
    public struct DamageBufferElement : IBufferElementData
    {
        [FormerlySerializedAs("Value")]
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
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(HitCheckSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct DamageDealingSystem : ISystem
    {
        private ComponentLookup<DestructTag> destructTagLp;
        private BufferLookup<DamageBufferElement> damageBufferLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            destructTagLp = SystemAPI.GetComponentLookup<DestructTag>(false);
            damageBufferLp = SystemAPI.GetBufferLookup<DamageBufferElement>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            destructTagLp.Update(ref state);
            damageBufferLp.Update(ref state);
            state.Dependency.Complete();
            new DealDamageJob() { damageBufferLp = damageBufferLp }.Run();
            new ApplyDamageJob() { destructTagLp = destructTagLp }.Run();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        /// <summary>
        /// add damage to target damage buffer according to damage dealer's component
        /// </summary>
        [BurstCompile]
        public partial struct DealDamageJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public BufferLookup<DamageBufferElement> damageBufferLp;
            public void Execute(
                [EntityIndexInQuery] int index,
                DynamicBuffer<HitCheckResultBufferElement> hitResults,
                in DealDamage damage)
            {
                for (int i = 0; i < hitResults.Length; i++)
                {
                    var target = hitResults[i].target;
                    if(!damageBufferLp.HasBuffer(target)) continue;
                    damageBufferLp[target].Add(new DamageBufferElement() { value = damage.value });
                }
            }
        }
        
        /// <summary>
        /// apply damage cached in damage buffer to hp. arrange destruct if hp fall to 0
        /// </summary>
        [BurstCompile]
        [WithDisabled(typeof(DestructTag))]
        public partial struct ApplyDamageJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<DestructTag> destructTagLp;
            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                DynamicBuffer<DamageBufferElement> damage,
                ref Hp hp)
            {
                for (int i = 0; i < damage.Length; i++)
                {
                    hp.current -= damage[i].value;
                }
                damage.Clear();
                if (hp.current<=0)
                {
                    destructTagLp.SetComponentEnabled(self,true);
                }
            }
        }
    }
}