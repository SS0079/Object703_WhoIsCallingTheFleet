using System;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Core.Combat
{
    #region ComponentData
    [Serializable]
    public struct HomingTarget : IComponentData
    {
        public Entity value;
    }
    
    [Serializable]
    public struct HomingCheckDistance : IComponentData
    {
        private bool init;
        private float sqr;
        [FormerlySerializedAs("_Raw")]
        [SerializeField]
        private float raw;
        public float Value
        {
            get => raw;
            set
            {
                raw = value;
                sqr = raw * raw;
            }
        }
        public float Sqr
        {
            get
            {
                if (!init)
                {
                    sqr = raw * raw;
                    init = true;
                }
                return sqr;
            }
        }
    }
    
    [Serializable]
    public struct HitCheckResultBufferElement : IBufferElementData
    {
        [FormerlySerializedAs("Value")]
        public Entity target;
        [FormerlySerializedAs("Point")]
        public float3 point;
    }
    
    [Serializable]
    public struct SphereOverlapCheck : IComponentData
    {
        [FormerlySerializedAs("Radius")]
        public float radius;
        public CollisionFilter filter;
        [Serializable]
        public struct AuthoringBox
        {
            [FormerlySerializedAs("Radius")]
            public float radius;
            [FormerlySerializedAs("Filter")]
            public PhysicsMaterialTemplate filter;

            public SphereOverlapCheck ToComponentData()
            {
                return new SphereOverlapCheck()
                {
                    radius = radius,
                    filter = new CollisionFilter
                    {
                        BelongsTo = filter.BelongsTo.Value,
                        CollidesWith = filter.CollidesWith.Value,
                        GroupIndex = filter.CustomTags.Value
                    }
                };
            }
        }
    }
    
    [Serializable]
    public struct SphereCastHitCheck : IComponentData
    {
        [FormerlySerializedAs("Radius")]
        public float radius;
        [FormerlySerializedAs("LastPos")]
        public float3 lastPos;
        public CollisionFilter filter;

        [Serializable]
        public struct AuthoringBox
        {
            [FormerlySerializedAs("Radius")]
            public float radius;
            [FormerlySerializedAs("LastPos")]
            public float3 lastPos;
            [FormerlySerializedAs("Filter")]
            public PhysicsMaterialTemplate filter;

            public SphereCastHitCheck ToComponentData()
            {
                return new SphereCastHitCheck
                {
                    radius = radius,
                    lastPos = default,
                    filter = new CollisionFilter
                    {
                        BelongsTo = filter.BelongsTo.Value,
                        CollidesWith = filter.CollidesWith.Value,
                        GroupIndex = filter.CustomTags.Value
                    }
                };
            }
        }
    }
            #endregion
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct HitCheckSystem : ISystem
    {
        // private ComponentLookup<DestructTag> destructTagLp;
        private ComponentLookup<LocalTransform> localTransLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            // destructTagLp = SystemAPI.GetComponentLookup<DestructTag>(false);
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            // destructTagLp.Update(ref state);
            localTransLp.Update(ref state);
            state.Dependency.Complete();
            new SphereCastHitCheckJob() { cWorld = cWorld}.ScheduleParallel();
            new SphereOverlapHitCheckJob() { cWorld = cWorld}.ScheduleParallel();
            new HomingShotHitCheckJob
            {
                localTransformLp = localTransLp,
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        /// <summary>
        /// perform hit check by a backward sphere cast. SphereCastHitCheck component hold the radius of sphere,collision filter and last position.
        /// cache the hit result in HitCheckResult buffer element
        /// </summary>
        [BurstCompile]
        [WithDisabled(typeof(DestructTag))]
        public partial struct SphereCastHitCheckJob : IJobEntity
        {
            [ReadOnly]
            public CollisionWorld cWorld;
            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                ref SphereCastHitCheck castHitCheck,
                in LocalTransform localTransform,
                DynamicBuffer<HitCheckResultBufferElement> hitResults,
                EnabledRefRW<DestructTag> destructTag)
            {
                castHitCheck.lastPos = castHitCheck.lastPos.Equals(float3.zero) ? localTransform.Position : castHitCheck.lastPos;
                var dir = math.normalizesafe(localTransform.Position-castHitCheck.lastPos);
                var length = math.distance(localTransform.Position, castHitCheck.lastPos);
                if (cWorld.SphereCast(castHitCheck.lastPos,castHitCheck.radius,dir,length,out ColliderCastHit hit,castHitCheck.filter))
                {
                   // destructTagLp.SetComponentEnabled(self, true);
                   destructTag.ValueRW = true;
                   hitResults.Add(new HitCheckResultBufferElement(){target = hit.Entity,point = hit.Position});
                }
                else
                {
                    castHitCheck.lastPos = localTransform.Position;
                }
            }
        }

        /// <summary>
        /// perform a sphere overlap hit check,the sphere overlap check component hold the radius of sphere and collision filter
        /// cache the hit result in HitCheckResult buffer element
        /// </summary>
        [BurstCompile]
        [WithDisabled(typeof(DestructTag))]
        public partial struct SphereOverlapHitCheckJob : IJobEntity
        {
            [ReadOnly]
            public CollisionWorld cWorld;
            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                in SphereOverlapCheck check,
                in LocalTransform localTransform,
                DynamicBuffer<HitCheckResultBufferElement> hitResults,
                EnabledRefRW<DestructTag> destructTag)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.TempJob);
                if (cWorld.OverlapSphere(localTransform.Position,check.radius,ref hits,check.filter))
                {
                    destructTag.ValueRW = true;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var localHit = hits[i];
                        hitResults.Add(new HitCheckResultBufferElement() { target = localHit.Entity, point = localHit.Position });
                    }
                }
            }
        }

        /// <summary>
        /// perform homing shot hit check by simply check the distance between entity and target
        /// </summary>
        [BurstCompile]
        [WithDisabled(typeof(DestructTag))]
        public partial struct HomingShotHitCheckJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> localTransformLp;

            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                in HomingTarget target,
                in LocalTransform localTransform,
                DynamicBuffer<HitCheckResultBufferElement> hitResults,
                in HomingCheckDistance fuze,
                EnabledRefRW<DestructTag> destructTag)

            {
                if (!localTransformLp.HasComponent(target.value)) return;
                if (math.distancesq(localTransform.Position, localTransformLp[target.value].Position) <= fuze.Sqr)
                {
                    destructTag.ValueRW = true;
                    hitResults.Add(new HitCheckResultBufferElement() { target = target.value, point = localTransformLp[target.value].Position });
                }
            }
        }
    }
    
    
}