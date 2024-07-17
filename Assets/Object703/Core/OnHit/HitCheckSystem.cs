﻿using System;
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

namespace Object703.Core.OnHit
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
    public struct HitCheckResult : IBufferElementData
    {
        public Entity target;
        public float3 point;
        public float3 normal;
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

    [Serializable]
    public struct MaxHitCount : IComponentData
    {
        public int value;
    }
    #endregion
    
    /// <summary>
    /// Contain logic about what happen on the frame when a weapon or skill hit a target
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(BeforeHitSystemGroup))]
    public partial struct HitCheckSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            localTransLp.Update(ref state);
            state.Dependency.Complete();
            new SphereCastHitCheckJob { cWorld = cWorld}.ScheduleParallel();
            new SphereOverlapHitCheckJob { cWorld = cWorld}.ScheduleParallel();
            new HomingShotHitCheckJob { localTransformLp = localTransLp }.ScheduleParallel();
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
        [WithNone(typeof(DestructTag))]
        public partial struct SphereCastHitCheckJob : IJobEntity
        {
            [ReadOnly]
            public CollisionWorld cWorld;
            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                ref SphereCastHitCheck castHitCheck,
                in LocalTransform localTransform,
                DynamicBuffer<HitCheckResult> hitResults,
                ref MaxHitCount count)
            {
                if(count.value<=0) return; 
                castHitCheck.lastPos = castHitCheck.lastPos.Equals(float3.zero) ? localTransform.Position : castHitCheck.lastPos;
                var dir = math.normalizesafe(localTransform.Position-castHitCheck.lastPos);
                var length = math.distance(localTransform.Position, castHitCheck.lastPos);
                var hitList = new NativeList<ColliderCastHit>(5, Allocator.TempJob);
                var curHitHash = new NativeHashSet<Entity>(hitResults.Length,Allocator.TempJob);
                for (int i = 0; i < hitResults.Length; i++)
                {
                    curHitHash.Add(hitResults[i].target);
                }
                if (cWorld.SphereCastAll(castHitCheck.lastPos,castHitCheck.radius,dir,length,ref hitList,castHitCheck.filter))
                {
                    for (int i = 0; i < hitList.Length; i++)
                    {
                        var item = hitList[i];
                        if(curHitHash.Contains(item.Entity)) continue;
                        hitResults.Add(new HitCheckResult { target = item.Entity, point = item.Position, normal = item.SurfaceNormal });
                        count.value--;
                    }
                }
                castHitCheck.lastPos = localTransform.Position;
            }
        }

        /// <summary>
        /// perform a sphere overlap hit check,the sphere overlap check component hold the radius of sphere and collision filter
        /// cache the hit result in HitCheckResult buffer element
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(DestructTag))]
        public partial struct SphereOverlapHitCheckJob : IJobEntity
        {
            [ReadOnly]
            public CollisionWorld cWorld;
            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                in SphereOverlapCheck check,
                in LocalTransform localTransform,
                DynamicBuffer<HitCheckResult> hitResults,
                ref MaxHitCount count
                )
            {
                if(count.value<=0) return;
                var hits = new NativeList<DistanceHit>(Allocator.TempJob);
                var curHitHash = new NativeHashSet<Entity>(hitResults.Length,Allocator.TempJob);
                for (int i = 0; i < hitResults.Length; i++)
                {
                    curHitHash.Add(hitResults[i].target);
                }
                if (cWorld.OverlapSphere(localTransform.Position,check.radius,ref hits,check.filter))
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var item = hits[i];
                        if(curHitHash.Contains(item.Entity)) continue;
                        hitResults.Add(new HitCheckResult() { target = item.Entity, point = item.Position });
                    }
                    count.value--;
                }
            }
        }

        /// <summary>
        /// perform homing shot hit check by simply check the distance between entity and target
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(DestructTag))]
        public partial struct HomingShotHitCheckJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> localTransformLp;

            public void Execute(
                [EntityIndexInQuery] int index,
                Entity self,
                in HomingTarget target,
                in LocalTransform localTransform,
                DynamicBuffer<HitCheckResult> hitResults,
                in HomingCheckDistance fuze,
                ref MaxHitCount count)

            {
                if (count.value<=0 || !localTransformLp.HasComponent(target.value)) return;
                if (math.distancesq(localTransform.Position, localTransformLp[target.value].Position) <= fuze.Sqr)
                {
                    hitResults.Add(new HitCheckResult() { target = target.value, point = localTransformLp[target.value].Position });
                    count.value--;
                }
            }
        }
    }

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnHitSystemGroup))]
    public partial struct OrderHitDestructSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (count,enDestruct) in SystemAPI
                         .Query<RefRO<MaxHitCount>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithDisabled<DestructTag>())
            {
                if (count.ValueRO.value<=0)
                {
                    enDestruct.ValueRW = true;
                }
            }
        }
    }
    
    
}