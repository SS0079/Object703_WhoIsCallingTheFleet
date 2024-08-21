using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace Object703.Core
{
    public struct TargetBuffer : ICommandData
    {
        [GhostField]public NetworkTick Tick { get; set; }
        [GhostField]public Entity value0;
        [GhostField]public float3 pos0;
        [GhostField]public Entity value1;
        [GhostField]public float3 pos1;
        [GhostField]public Entity value2;
        [GhostField]public float3 pos2;
        [GhostField]public Entity value3;
        [GhostField]public float3 pos3;
        [GhostField]public Entity value4;
        [GhostField]public float3 pos4;
        [GhostField]public Entity value5;
        [GhostField]public float3 pos5;
        [GhostField]public Entity value6;
        [GhostField]public float3 pos6;
        [GhostField]public Entity value7;
        [GhostField]public float3 pos7;

        public static TargetBuffer GetInvalid()
        {
            var result = new TargetBuffer();
            result.value0=Entity.Null;
            result.value1=Entity.Null;
            result.value2=Entity.Null;
            result.value3=Entity.Null;
            result.value4=Entity.Null;
            result.value5=Entity.Null;
            result.value6=Entity.Null;
            result.value7=Entity.Null;
            return result;
        }

        public (Entity, float3) this[int key]
        {
            get
            {
                switch (key)
                {
                    case 0:
                        return (value0, pos0);
                    case 1:
                        return (value1, pos1);
                    case 2:
                        return (value2, pos2);
                    case 3:
                        return (value3, pos3);
                    case 4:
                        return (value4, pos4);
                    case 5:
                        return (value5, pos5);
                    case 6:
                        return (value6, pos6);
                    case 7:
                        return (value7, pos7);
                    default:
                        return (Entity.Null, default);
                }
            }
            set
            {
                switch (key)
                {
                    case 0:
                        value0 = value.Item1;
                        pos0 = value.Item2;
                        break;
                    case 1:
                        value1 = value.Item1;
                        pos1 = value.Item2;
                        break;
                    case 2:
                        value2 = value.Item1;
                        pos2 = value.Item2;
                        break;
                    case 3:
                        value3 = value.Item1;
                        pos3 = value.Item2;
                        break;
                    case 4:
                        value4 = value.Item1;
                        pos4 = value.Item2;
                        break;
                    case 5:
                        value5 = value.Item1;
                        pos5 = value.Item2;
                        break;
                    case 6:
                        value6 = value.Item1;
                        pos6 = value.Item2;
                        break;
                    case 7:
                        value7 = value.Item1;
                        pos7 = value.Item2;
                        break;
                    default:
                        break;
                }
            }
        }
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnPredicatedShootSystemGroup),OrderFirst = true)]
    public partial struct WeaponTargetingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            new CleanUpTarget().Run();
            new TargetingJob() { cWorld = cworld }.Run();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        /// <summary>
        /// all target buffer element must be clean up before any targeting job been perform
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(DestructTag))]
        public partial struct CleanUpTarget : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                DynamicBuffer<TargetBuffer> targets)
            {
                targets.AddCommandData(TargetBuffer.GetInvalid());
            }
        }
        
        
        /// <summary>
        /// scan airborne target and cache them in weapon target buffer
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(DestructTag))]
        public partial struct TargetingJob : IJobEntity
        {
            [ReadOnly]
            public CollisionWorld cWorld;
            public void Execute(
                [EntityIndexInQuery] int index,
                DynamicBuffer<TargetBuffer> targets,
                in Weapon weapon,
                in LocalToWorld ltw)
            {
                NativeList<DistanceHit> outHits=new NativeList<DistanceHit>(Allocator.TempJob);
                // var aabbOffset = new float3(weapon.maxRange, 20, weapon.maxRange);
                // Aabb aabb = new Aabb
                // {
                //     Min = ltw.Position - aabbOffset,
                //     Max = ltw.Position+aabbOffset
                // };
                // var aabbInput=new OverlapAabbInput
                // {
                //     Aabb = aabb,
                //     Filter = weapon.targetFilter
                // };
                
                // TODO: rewrite it with overlap aabb
                var hit = cWorld.OverlapBox(
                    ltw.Position,
                    default,
                    new float3(weapon.maxRange,25,weapon.maxRange),
                    ref outHits,
                    weapon.targetFilter);
                if (!hit) return;
                var targetCount = math.min(weapon.targetCount, 8);
                var newTargets = new TargetBuffer();
                for (int i = 0,j=0; i < outHits.Length && j < targetCount; i++)
                {
                    var hitResult = outHits[i];
                    if (WithInRange(weapon,ltw.Position,ltw.Forward,hitResult.Position))
                    {
                        newTargets[j] = (hitResult.Entity, hitResult.Position);
                        j++;
                    }
                }
                targets.AddCommandData(newTargets);
            }
        }
        
        private static bool WithInRange(Weapon weapon, float3 muzzlePos,float3 muzzleFwd, float3 targetPos)
        {
            var disSq = math.distancesq(muzzlePos, targetPos);
            if (disSq<weapon.minRange*weapon.minRange || disSq>weapon.maxRange*weapon.maxRange)//return false if too close
            {
                return false;
            }
            targetPos = new float3(targetPos.x, 0, targetPos.z);
            muzzlePos = new float3(muzzlePos.x, 0, muzzlePos.z);
            muzzleFwd = new float3(muzzleFwd.x, 0, muzzleFwd.z);
            var targetDir = math.normalizesafe(targetPos - muzzlePos);
            var dot = math.dot(muzzleFwd, targetDir);
            if (dot<weapon.ArcLimitCos)//return false if not in arc
            {
                return false;
            }
            return true;
        }

        
        /// <summary>
        /// reorder native list of distanceHit in the distance ascend order
        /// </summary>
        private static void OrderByDisAscend(ref NativeList<DistanceHit> pos)
        {
            DistanceHit temp;
            for (int i = 0; i < pos.Length; i++)
            {
                for (int j = pos.Length-1; j > i; j--)
                {
                    if (pos[j].Distance<pos[j-1].Distance)
                    {
                        temp = pos[j];
                        pos[j] = pos[j - 1];
                        pos[j - 1] = temp;
                    }
                }
            }
        }
    }
}