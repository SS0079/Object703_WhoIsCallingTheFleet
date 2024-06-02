using System;
using Object703.Core.Combat;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

namespace Object703.Core.Moving
{
    #region ComponentData
    public struct ShipMoveConfig : IComponentData
    {
        [GhostField]
        public float moveSpeedPerTick;
        [Range(0,300)]
        [GhostField]
        public float moveDampMotion;
        [Range(0,300)]
        [GhostField]
        public float moveDampStop;
        private float rotateRadiusPerTick;
        public float RotateRadiusPerTick
        {
            get => rotateRadiusPerTick;
            set => rotateRadiusPerTick = math.radians(value);
        }
        public float rotateDampMotion;
        public float rotateDampStop;
        
        [Serializable]
        public struct AuthoringBox
        {
            public float moveSpeedPerSecond;
            [Range(0,300)]
            public float moveDampMotion;
            [Range(0,300)]
            public float moveDampStop;
            public float rotateDegreePerSecond;
            [Range(0,300)]
            public float rotateDampMotion;
            [Range(0,300)]
            public float rotateDampStop;
            public ShipMoveConfig ToComponentData(NetCodeConfig config) => new ShipMoveConfig
            {
                moveSpeedPerTick = moveSpeedPerSecond/config.ClientServerTickRate.SimulationTickRate,
                moveDampMotion = moveDampMotion,
                moveDampStop = moveDampStop,
                RotateRadiusPerTick = rotateDegreePerSecond/config.ClientServerTickRate.SimulationTickRate,
                rotateDampMotion = rotateDampMotion,
                rotateDampStop = rotateDampStop
            };
        }
    }
    
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct RotateSpeed : IComponentData
    {
        [GhostField(Quantization = 1000)]
        public float3 value;
    }
    
    [Serializable]
    public struct RotateAxis : IComponentData
    {
        public float3 rotateEuler;
    }
    
    // [Serializable]
    // public struct MoveSpeedConfig : IComponentData
    // {
    //     
    // }
    
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct MoveSpeed : IComponentData
    {
        [GhostField(Quantization = 1000)]
        public float3 value;
    }
    
    [Serializable]
    public struct MoveAsShipTag : IComponentData
    {
        
    }
    
    [Serializable]
    public struct MoveAxis : IComponentData
    {
        public float3 moveDirection;
    }
    
    public enum ShipMoveType
    {
        Ship,
        Hover
    }
    [Serializable]
    public struct ArrowMoveConfig : IComponentData
    {
        [GhostField]public float speedPerTick;
        public struct AuthoringBox
        {
            public float speedPerSecond;
            public ArrowMoveConfig ToComponentData(NetCodeConfig config)
            {
                return new ArrowMoveConfig() { speedPerTick = speedPerSecond / config.ClientServerTickRate.SimulationTickRate };
            }
        }
    }
    #endregion

    public partial struct MoveSystemJobs
    {
        /// <summary>
        /// perform a moving behaviour that move a entity like a ship
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(MoveAsShipTag))]
        public partial struct ShipMoveJob : IJobEntity
        {
            // public float Δt;
            public void Execute(
                [EntityIndexInQuery] int index,
                ref LocalTransform localTransform,
                in ShipMoveConfig moveConfig,
                in MoveAxis moveAxis,
                in RotateAxis rotateAxis,
                ref MoveSpeed moveSpeed,
                ref RotateSpeed rotateSpeed)
            {
                var targetEuler = rotateAxis.rotateEuler*moveConfig.RotateRadiusPerTick;
                var rotateDamp =rotateAxis.rotateEuler.y==0 ? moveConfig.rotateDampStop : moveConfig.rotateDampMotion;
                rotateSpeed.value = math.lerp(rotateSpeed.value, targetEuler, 1f/ (rotateDamp+1));
                var quaternion = Unity.Mathematics.quaternion.Euler(rotateSpeed.value);
                localTransform=localTransform.Rotate(quaternion);
                
                var targetSpeed = localTransform.TransformDirection(moveAxis.moveDirection);
                var speed=moveConfig.moveSpeedPerTick * targetSpeed;
                var speedDamp = math.lengthsq(moveAxis.moveDirection)==0 ? moveConfig.moveDampStop : moveConfig.moveDampMotion;
                moveSpeed.value = math.lerp(moveSpeed.value, speed, 1f / (speedDamp + 1));
                localTransform.Position += new float3(moveSpeed.value);
            }
        }

        /// <summary>
        /// perform a moving behaviour that a entity just move forward
        /// </summary>
        [BurstCompile]
        [WithDisabled(typeof(DestructTag))]
        public partial struct ProjectileMoveJob : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                ref LocalTransform localTransform,
                in ArrowMoveConfig arrowMoveConfig,
                in LocalToWorld ltw
                )
            {
                localTransform.Position += ltw.Forward * arrowMoveConfig.speedPerTick;
            }
        }

        /// <summary>
        /// perform a look at behaviour that force entity look at a homing target
        /// </summary>
        [BurstCompile]
        public partial struct HomingJob : IJobEntity
        {
            public HomingJob(ComponentLookup<LocalToWorld> ltwLp, ComponentLookup<Simulate> simulateLp) : this()
            {
                this.ltwLp = ltwLp;
                this.simulateLp = simulateLp;
            }

            [ReadOnly]
            private ComponentLookup<LocalToWorld> ltwLp;
            [ReadOnly]
            private ComponentLookup<Simulate> simulateLp;
            public void Execute(
                [EntityIndexInQuery] int index,
                in HomingTarget target,
                ref LocalTransform localTransform)
            {
                if (!simulateLp.HasComponent(target.value) || !ltwLp.HasComponent(target.value)) return;
                var targetPos = ltwLp[target.value].Position;
                var targetLookRot = quaternion.LookRotationSafe(targetPos - localTransform.Position, localTransform.Up());
                localTransform.Rotation = targetLookRot;
            }
        }
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    // [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MoveSystem : ISystem
    {
        private ComponentLookup<LocalToWorld> ltwLp;
        private ComponentLookup<Simulate> simulateLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            ltwLp = SystemAPI.GetComponentLookup<LocalToWorld>(true);
            simulateLp = SystemAPI.GetComponentLookup<Simulate>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var Δt = SystemAPI.Time.DeltaTime;
            var shipMoveHandle = new MoveSystemJobs.ShipMoveJob().ScheduleParallel(state.Dependency);
            shipMoveHandle.Complete();
            
            new MoveSystemJobs.ProjectileMoveJob().ScheduleParallel();
            ltwLp.Update(ref state);
            simulateLp.Update(ref state);
            new MoveSystemJobs.HomingJob(ltwLp, simulateLp).ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

    }
    
}