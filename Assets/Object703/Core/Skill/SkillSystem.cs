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
    public struct TeleportSkill : IComponentData { }
    public struct ShotSkill : IComponentData
    {
        public Entity charge;
    }
    [Serializable]
    public struct SkillCommonData : IComponentData
    {
        [GhostField]
        public float radius;
        [GhostField]
        public float range;
        [GhostField]
        public uint coolDownTick;
        [GhostField]
        public uint lifeSpanTick;

        public float RangeSq => range * range;
        
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
                    range = range,
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
    
    [Serializable]
    public struct SkillFlags : IInputComponentData
    {
        [FormerlySerializedAs("Slot")]
        public SkillSlot slot;
        [FormerlySerializedAs("fireSkill")]
        [GhostField]
        public SkillStatus status;
    }

    public enum SkillStatus
    {
        Ready,
        Firing,
        Cooldown,
        Fired,
    }
    
    // [BurstCompile]
    // [RequireMatchingQueriesForUpdate]
    // [UpdateInGroup(typeof(SimulationSystemGroup))]
    // public partial struct PrepareSkillSystem : ISystem
    // {
    //     public void OnCreate(ref SystemState state)
    //     {
    //         state.RequireForUpdate<NetworkTime>();
    //     }
    //
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         //update the target tick of skill
    //         var networkTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
    //   
    //     }
    // }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SkillSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransLp;
        private ComponentLookup<PlayerInput> inputLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(false);
            inputLp = SystemAPI.GetComponentLookup<PlayerInput>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            localTransLp.Update(ref state);
            inputLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            foreach (var (commonData,invokeAtTick,flags) in SystemAPI
                         .Query<RefRO<SkillCommonData>,DynamicBuffer<SkillInvokeAtTick>,RefRW<SkillFlags>>().WithAll<Simulate>())
            {
                if(flags.ValueRO.status!=SkillStatus.Fired) continue;
                var localTick = networkTime.ServerTick;
                var newTickCommand = new SkillInvokeAtTick();
                newTickCommand.Tick = networkTime.ServerTick;
                localTick.Add(commonData.ValueRO.coolDownTick);
                newTickCommand.coolDownAtTick = localTick;
                localTick = networkTime.ServerTick;
                localTick.Add(commonData.ValueRO.lifeSpanTick);
                newTickCommand.lifeSpanAtTick = localTick;
                invokeAtTick.AddCommandData(newTickCommand);
                flags.ValueRW.status = SkillStatus.Cooldown;
            }
            
            // var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            //first we check if skill is finish cool down and relate input key is pressed (in parent), if so , set the fireSkill field to true, set to false if not.
            //Then invoke each skill according to the fireSkill field

            foreach (var (flags,invokeAtTick,parent) in SystemAPI
                         .Query<RefRW<SkillFlags>,DynamicBuffer<SkillInvokeAtTick>,RefRO<Parent>>().WithAll<Simulate>())
            {
                // fired skill status indicate cooldown tick havent been reset after previous skill execution. wait until it reset
                if(flags.ValueRO.status==SkillStatus.Fired) continue;
                var parentEntity = parent.ValueRO.Value;
                if(!inputLp.HasComponent(parentEntity)) continue;
                //set false and break if skill is still in cool down
                invokeAtTick.GetDataAtTick(networkTime.ServerTick, out var coolDownTick);
                if (!networkTime.ServerTick.IsNewerThan(coolDownTick.coolDownAtTick))
                {
                    flags.ValueRW.status = SkillStatus.Cooldown;
                    continue;
                }
                //check input according to skill flag slot. if related key is not pressed, set false and break
                var playerInput = inputLp[parentEntity];
                switch (flags.ValueRO.slot)
                {
                    case SkillSlot.Skill0:
                        flags.ValueRW.status = playerInput.skill0.IsSet? SkillStatus.Firing : SkillStatus.Ready;
                        break;
                    case SkillSlot.Skill1:
                        flags.ValueRW.status = playerInput.skill1.IsSet? SkillStatus.Firing : SkillStatus.Ready;
                        break;
                    case SkillSlot.Skill2:
                        flags.ValueRW.status = playerInput.skill2.IsSet? SkillStatus.Firing : SkillStatus.Ready;
                        break;
                    case SkillSlot.Skill3:
                        flags.ValueRW.status = playerInput.skill3.IsSet? SkillStatus.Firing : SkillStatus.Ready;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            //perform teleport skill,also check if mouse aim is in skill range. perform skill at maximum range if mouse aim is out of range
            foreach (var (commonData,parent,flags) in SystemAPI.Query<RefRW<SkillCommonData>,RefRO<Parent>,RefRW<SkillFlags>>().WithAll<Simulate,TeleportSkill>())
            {
                if(flags.ValueRO.status!=SkillStatus.Firing) continue;
                var performer = parent.ValueRO.Value;
                if(!inputLp.HasComponent(performer)) continue;
                var playerInput = inputLp[performer];
                
                if (!localTransLp.HasComponent(performer)) return;
                var performerPos = playerInput.playerPosition;
                var targetPos = playerInput.mouseWorldPoint;
                var distancesq = math.distancesq(performerPos,targetPos);
                //clamp target position according to skill range
                if (distancesq>commonData.ValueRO.RangeSq)
                {
                    var targetDir = math.normalizesafe(targetPos-performerPos);
                    targetPos = performerPos + targetDir * commonData.ValueRO.range;
                }
                var newPos = new float3(targetPos.x, performerPos.y, targetPos.z);
                var newRot = quaternion.LookRotationSafe(newPos - performerPos, math.up());
                var newTrans = LocalTransform.FromPositionRotation(newPos,newRot);
                localTransLp[performer] = newTrans;

                flags.ValueRW.status = SkillStatus.Fired;
            }
            
            if (networkTime.IsFirstTimeFullyPredictingTick)
            {
                // perform shot skill, also check if aim target is in skill range. if not, dont fire skill, and set skill flag to false, avoid later skill target tick reset
                foreach (var (shot,commonData,owner,flags,ltw,parent,entity) in 
                         SystemAPI.Query<RefRO<ShotSkill>,
                                 RefRO<SkillCommonData>,
                                 RefRW<GhostOwner>,
                                 RefRW<SkillFlags>,
                                 RefRO<LocalToWorld>,
                                 RefRO<Parent>>()
                             .WithAll<Simulate>().WithEntityAccess())
                {
                    if (flags.ValueRO.status!=SkillStatus.Firing) continue;
                    var performer = parent.ValueRO.Value;
                    if(!inputLp.HasComponent(performer)) continue;
                    var playerInput = inputLp[performer];
                    // stop fire skill if out of range
                    var performerPos = playerInput.playerPosition;
                    var distancesq = playerInput.GetSqDstFromPlayerToMouseEntity2D(localTransLp);
                    if (distancesq>commonData.ValueRO.RangeSq)
                    {
                        flags.ValueRW.status = SkillStatus.Ready;
                        continue;
                    }
                    var localShot=state.EntityManager.Instantiate(shot.ValueRO.charge);
                    //force shot towards target position
                    var rot = quaternion.LookRotationSafe(playerInput.mouseWorldPoint-playerInput.playerPosition,math.up());
                    var localTrans = LocalTransform.FromPositionRotation(performerPos, rot);
                    SystemAPI.SetComponent(localShot,owner.ValueRO);
                    SystemAPI.SetComponent(localShot, localTrans);

                    flags.ValueRW.status = SkillStatus.Fired;
                }
                
            }
            //
            // foreach (var (flags,invokeAtTick,commonData) in SystemAPI
            //              .Query<RefRW<SkillFlags>,RefRW<SkillInvokeAtTick>,RefRO<SkillCommonData>>().WithAll<Simulate>())
            // {
            //     flags.ValueRW.status = false;
            //     var nextTick = networkTime.ServerTick;
            //     nextTick.Add(commonData.ValueRO.coolDownTick);
            //     invokeAtTick.ValueRW.coolDownAtTick = nextTick;
            // }
            
            // ecb.Playback(state.EntityManager);
            // ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        

    }
}