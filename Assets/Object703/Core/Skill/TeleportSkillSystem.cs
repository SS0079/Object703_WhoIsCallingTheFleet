using System;
using KittyHelpYouOut.Utilities;
using Object703.Core.Combat;
using Object703.Core.Control;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Core.Skill
{
    [Serializable]
    public struct SkillFlags : IComponentData
    {
        [FormerlySerializedAs("Slot")]
        public SkillSlot slot;
    }
    
    public struct TeleportSkill : IComponentData { }

    [Serializable]
    public struct SkillCommonData : IComponentData
    {
        [GhostField]
        public float radius;
        /// <summary>
        /// Do not access directly
        /// </summary>
        [GhostField]
        public float _range;
        private float rangeSq;
        public float Range
        {
            get => _range;
            set
            {
                _range = value;
                rangeSq = _range * _range;
            }
        }
        public float RangeSq => rangeSq;
        [GhostField]
        public uint coolDownTick;
        [GhostField]
        public uint lifeSpanTick;

        
        [Serializable]
        public struct AuthoringBox
        {
            public float radius;
            public float range;
            public float coolDown;
            public float lifeSpan;
            // public bool fireSkillOutOfRange;
            public SkillCommonData ToComponentData(int tickRate)
            {
                return new SkillCommonData()
                {
                    radius = radius,
                    Range = range,
                    coolDownTick = (uint)(coolDown * tickRate),
                    lifeSpanTick = (uint)(coolDown * lifeSpan),
                };
            }
        }
    }

    public struct SkillInvokeAtTick : ICommandData
    {
        [GhostField]
        public NetworkTick Tick { get; set; }
        [GhostField]
        public NetworkTick coolDownAtTick;
        [GhostField]
        public NetworkTick lifeSpanAtTick;

    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct TeleportSkillSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
                state.RequireForUpdate<NetworkTime>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            localTransLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            //perform teleport skill,also check if mouse aim is in skill range. perform skill at maximum range if mouse aim is out of range
            foreach (var skill in 
                     SystemAPI.Query<SkillAspect>()
                         .WithAll<Simulate,TeleportSkill>())
            {
                if(!skill.IsReady(networkTime)) continue;
                if(!skill.IsPressed()) continue;
                
                //clamp target position according to skill range
                var newPos = skill.AimPos;
                var skillOwnerPos = skill.OwnerPos;
                if (!skill.IsInRange(skill.AimDistanceSq))
                {
                    var targetDir = math.normalizesafe(newPos-skillOwnerPos);
                    newPos = skillOwnerPos + targetDir * skill.Range;
                }
                newPos = new float3(newPos.x, skillOwnerPos.y, newPos.z);
                var newRot = quaternion.LookRotationSafe(newPos - skillOwnerPos, math.up());
                var newTrans = LocalTransform.FromPositionRotation(newPos,newRot);
                localTransLp[skill.OwnerEntity] = newTrans;
            
                if(state.WorldUnmanaged.IsServer()) continue;
                skill.StartCoolDown(networkTime.ServerTick);
            }
        }


        

    }
}