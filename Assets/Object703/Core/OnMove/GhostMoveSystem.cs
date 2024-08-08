using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using IJobEntity = Unity.Entities.IJobEntity;

namespace Object703.Core
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
            public ShipMoveConfig ToComponentData(int tickRate) => new ShipMoveConfig
            {
                moveSpeedPerTick = moveSpeedPerSecond/tickRate,
                moveDampMotion = moveDampMotion,
                moveDampStop = moveDampStop,
                RotateRadiusPerTick = rotateDegreePerSecond/tickRate,
                rotateDampMotion = rotateDampMotion,
                rotateDampStop = rotateDampStop
            };
        }
    }
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct RotateSpeed : IInputComponentData
    {
        [GhostField(Quantization = 1000)]
        public float3 value;
    }
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct RotateAxis : IInputComponentData
    {
        [GhostField] public float3 rotateEuler;
    }
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct MoveSpeed : IInputComponentData
    {
        [GhostField(Quantization = 1000)]
        public float3 value;
    }
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct MoveAxis : IInputComponentData
    {
        [GhostField] public float3 moveDirection;
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
        
        [Serializable]
        public struct AuthoringBox
        {
            public float speedPerSecond;
            public ArrowMoveConfig ToComponentData(int tickRate)
            {
                return new ArrowMoveConfig() { speedPerTick = speedPerSecond / tickRate };
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
        [WithAll(typeof(Simulate),typeof(GhostOwner))]
        [WithNone(types: typeof(DestructTag))]
        public partial struct ShipMoveJob : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                ref LocalTransform trans,
                in ShipMoveConfig moveConfig,
                in MoveAxis moveAxis,
                in RotateAxis rotateAxis,
                ref MoveSpeed moveSpeed,
                ref RotateSpeed rotateSpeed)
            {
                var targetEuler = rotateAxis.rotateEuler*moveConfig.RotateRadiusPerTick;
                var rotateDamp =rotateAxis.rotateEuler.y==0 ? moveConfig.rotateDampStop : moveConfig.rotateDampMotion;
                rotateSpeed.value = math.lerp(rotateSpeed.value, targetEuler, 1f/ (rotateDamp+1));
                var q = quaternion.Euler(rotateSpeed.value);
                trans=trans.Rotate(q);
                
                var speedDir = trans.TransformDirection(moveAxis.moveDirection);
                var speed=moveConfig.moveSpeedPerTick * speedDir;
                var speedDamp = math.lengthsq(moveAxis.moveDirection)==0 ? moveConfig.moveDampStop : moveConfig.moveDampMotion;
                moveSpeed.value = math.lerp(moveSpeed.value, speed, 1f / (speedDamp + 1));
                trans.Position += new float3(moveSpeed.value);
            }
        }
    
        /// <summary>
        /// perform a moving behaviour that a entity just move forward
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(Simulate))]
        [WithNone(typeof(DestructTag))]
        public partial struct ArrowMoveJob : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                ref LocalTransform trans,
                in ArrowMoveConfig moveConfig,
                in LocalToWorld ltw
                )
            {
                trans.Position += ltw.Forward * moveConfig.speedPerTick;
            }
        }
    
        // /// <summary>
        // /// perform a look at behaviour that force entity look at a homing target
        // /// </summary>
        // [BurstCompile]
        // public partial struct HomingJob : IJobEntity
        // {
        //     public HomingJob(ComponentLookup<LocalToWorld> ltwLp, ComponentLookup<Simulate> simulateLp) : this()
        //     {
        //         this.ltwLp = ltwLp;
        //         this.simulateLp = simulateLp;
        //     }
        //
        //     [ReadOnly]
        //     private ComponentLookup<LocalToWorld> ltwLp;
        //     [ReadOnly]
        //     private ComponentLookup<Simulate> simulateLp;
        //     public void Execute(
        //         [EntityIndexInQuery] int index,
        //         in HomingTarget target,
        //         ref LocalTransform localTransform)
        //     {
        //         if (!simulateLp.HasComponent(target.value) || !ltwLp.HasComponent(target.value)) return;
        //         var targetPos = ltwLp[target.value].Position;
        //         var targetLookRot = quaternion.LookRotationSafe(targetPos - localTransform.Position, localTransform.Up());
        //         localTransform.Rotation = targetLookRot;
        //     }
        // }
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnPredicatedMoveSystemGroup))]
    public partial struct GhostMoveSystem : ISystem
    {

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency();
            state.Dependency = new MoveSystemJobs.ShipMoveJob().ScheduleParallel(state.Dependency);
            state.Dependency = new MoveSystemJobs.ArrowMoveJob().ScheduleParallel(state.Dependency);
        }

    }
    
}