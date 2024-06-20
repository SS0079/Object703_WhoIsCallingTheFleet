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
        private ComponentLookup<PlayerMoveInput> inputLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
                state.RequireForUpdate<NetworkTime>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(false);
            inputLp = SystemAPI.GetComponentLookup<PlayerMoveInput>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            localTransLp.Update(ref state);
            inputLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            if(!networkTime.IsFirstTimeFullyPredictingTick) return;
            // //perform teleport skill,also check if mouse aim is in skill range. perform skill at maximum range if mouse aim is out of range
            // foreach (var (skill,parent) in 
            //          SystemAPI.Query<SkillAspect,
            //              RefRO<Parent>>()
            //              .WithAll<Simulate,TeleportSkill>())
            // {
            //     if(!skill.IsReady(networkTime.ServerTick)) continue;
            //     var performer = parent.ValueRO.Value;
            //     if(!inputLp.HasComponent(performer)) continue;
            //     var playerInput = inputLp[performer];
            //     if(!skill.IsPressed(playerInput)) continue;
            //     
            //     var performerPos = playerInput.playerPosition;
            //     var targetPos = playerInput.mouseWorldPoint;
            //     var distancesq = math.distancesq(performerPos,targetPos);
            //     //clamp target position according to skill range
            //     if (!skill.IsInRange(distancesq))
            //     {
            //         var targetDir = math.normalizesafe(targetPos-performerPos);
            //         targetPos = performerPos + targetDir * skill.Range;
            //     }
            //     var newPos = new float3(targetPos.x, performerPos.y, targetPos.z);
            //     var newRot = quaternion.LookRotationSafe(newPos - performerPos, math.up());
            //     var newTrans = LocalTransform.FromPositionRotation(newPos,newRot);
            //     // ecb.SetComponent(performer,newTrans);
            //     localTransLp[performer] = newTrans;
            //
            //     skill.StartCoolDown(networkTime.ServerTick);
            // }
            foreach (var (skill,input,trans,entity) in 
                     SystemAPI.Query<SkillAspect,
                             PlayerSkillInput
                         ,RefRW<LocalTransform>>()
                         .WithAll<Simulate,TeleportSkill>().WithEntityAccess())
            {
                if(!skill.IsReady(networkTime)) continue;
                if(!skill.IsPressed(input)) continue;
                
                var performerPos = input.playerPosition;
                var targetPos = input.mouseWorldPoint;
                var distancesq = math.distancesq(performerPos,targetPos);
                //clamp target position according to skill range
                if (!skill.IsInRange(distancesq))
                {
                    var targetDir = math.normalizesafe(targetPos-performerPos);
                    targetPos = performerPos + targetDir * skill.Range;
                }
                var newPos = new float3(targetPos.x, performerPos.y, targetPos.z);
                var newRot = quaternion.LookRotationSafe(newPos - performerPos, math.up());
                var newTrans = LocalTransform.FromPositionRotation(newPos,newRot);
                trans.ValueRW = newTrans;
                // ecb.SetComponent(entity,newTrans);
                
                if(state.WorldUnmanaged.IsServer()) continue;
                skill.StartCoolDown(networkTime.ServerTick);
            }
            
        }


        

    }
}