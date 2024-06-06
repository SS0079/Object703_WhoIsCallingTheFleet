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

            public SkillCommonData ToComponentData(int tickRate)
            {
                return new SkillCommonData()
                {
                    radius = radius,
                    range = range,
                    coolDownTick = (uint)(coolDown * tickRate),
                    lifeSpanTick = (uint)(coolDown * lifeSpan)
                };
            }
        }
    }

    public struct SkillInvokeAtTick : IComponentData , IEnableableComponent
    {
        public NetworkTick coolDownAtTick;
        public NetworkTick lifeSpanAtTick;
    }
    
    [Serializable]
    public struct SkillFlags : IComponentData
    {
        [FormerlySerializedAs("Slot")]
        public SkillSlot slot;
        [FormerlySerializedAs("SkillTriggerDown")]
        public InputEvent skillTriggerDown;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(SkillSystem))]
    public partial struct PreSkillSystem : ISystem
    {
        private ComponentLookup<PlayerInput> inputLp;
        public void OnCreate(ref SystemState state)
        {
            inputLp = SystemAPI.GetComponentLookup<PlayerInput>(true);

        }

        public void OnUpdate(ref SystemState state)
        {
            var Δt = SystemAPI.Time.fixedDeltaTime;
            inputLp.Update(ref state);
            // update skill flags
            foreach (var (flags,commonData,parent) in SystemAPI.Query<RefRW<SkillFlags>,RefRO<SkillCommonData>,RefRO<Parent>>().WithAll<Simulate>())
            {
                if(!inputLp.HasComponent(parent.ValueRO.Value)) continue;
                var playerInput = inputLp[parent.ValueRO.Value];

                flags.ValueRW.Tick(Δt);
            }
        }
    }
    
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
            var Δt = SystemAPI.Time.DeltaTime;
            localTransLp.Update(ref state);
            inputLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // new SkillSystemJobs.UpdateSkillFlagsJob() { Δt = Δt }.Run();
            
            // // update skill flags
            // foreach (var (flags,commonData,parent) in SystemAPI.Query<RefRW<SkillFlags>,RefRW<SkillCommonData>,RefRO<Parent>>().WithAll<Simulate>())
            // {
            //     if(!inputLp.HasComponent(parent.ValueRO.Value)) continue;
            //     // flags.ValueRW.skillPermission = false;
            //     var playerInput = inputLp[parent.ValueRO.Value];
            //     
            //     if (commonData.ValueRW.timer<0)
            //     {
            //         if (flags.ValueRO.skillTriggerDown)
            //         {
            //             //check range
            //             
            //             if (playerInput.fromTo2DDisSq>commonData.ValueRO.RangeSq) continue;
            //             flags.ValueRW.skillPermission = true;
            //             commonData.ValueRW.timer = commonData.ValueRO.coolDown;
            //             Debug.Log($"{commonData.ValueRO.timer} | {flags.ValueRO.skillPermission}");
            //         }
            //     }
            //     else
            //     {
            //         commonData.ValueRW.timer -= Δt;
            //     }
            // }
            
            //perform teleport skill
            foreach (var (commonData,parent,flags) in SystemAPI.Query<RefRW<SkillCommonData>,RefRO<Parent>,RefRW<SkillFlags>>().WithAll<Simulate,TeleportSkill>())
            {
                if(!flags.ValueRO.Activate) continue;
                var performer = parent.ValueRO.Value;
                if(!inputLp.HasComponent(performer)) continue;
                var playerInput = inputLp[performer];
                
                if (!localTransLp.HasComponent(performer)) return;
                var previousPos = playerInput.playerPosition;
                var dest = playerInput.mouseWorldPoint;
                var newPos = new float3(dest.x, previousPos.y, dest.z);
                var newRot = quaternion.LookRotationSafe(newPos - previousPos, math.up());
                var newTrans = LocalTransform.FromPositionRotation(newPos,newRot);
                localTransLp[performer] = newTrans;
                flags.ValueRW.Reset(commonData.ValueRO.coolDown);
                
            }
            
            if (networkTime.IsFirstTimeFullyPredictingTick)
            {
                // perform shot skill
                foreach (var (shot,commonData,owner,flags,ltw,parent,entity) in 
                         SystemAPI.Query<RefRO<ShotSkill>,
                                 RefRO<SkillCommonData>,
                                 RefRW<GhostOwner>,
                                 RefRW<SkillFlags>,
                                 RefRO<LocalToWorld>,
                                 RefRO<Parent>>()
                             .WithAll<Simulate>().WithEntityAccess())
                {
                    if (!flags.ValueRO.Activate) continue;
                    var performer = parent.ValueRO.Value;
                    if(!inputLp.HasComponent(performer)) continue;
                    var playerInput = inputLp[performer];
                    var localShot=state.EntityManager.Instantiate(shot.ValueRO.charge);
                    
                    //force shot towards target position
                    var rot = quaternion.LookRotationSafe(playerInput.mouseWorldPoint-playerInput.playerPosition,math.up());
                    var localTrans = new LocalTransform
                    {
                        Position = ltw.ValueRO.Position,
                        Scale = 1,
                        Rotation = rot
                    };
                    SystemAPI.SetComponent(localShot,owner.ValueRO);
                    SystemAPI.SetComponent(localShot, localTrans);
                    flags.ValueRW.Reset(commonData.ValueRO.coolDown);
                    // Debug.Log($"{state.WorldUnmanaged.Name} : {entity.Index} | {data.ValueRO.timer} | {flags.ValueRO.skillPermission}");
                }
                
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        

    }
}