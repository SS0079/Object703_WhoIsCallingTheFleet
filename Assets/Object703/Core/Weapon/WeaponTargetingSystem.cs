using System;
using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace Object703.Core.Weapon
{
    [Serializable]
    [InternalBufferCapacity(64)]
    public struct TargetBufferElement : IBufferElementData
    {
        public Entity value;
        public float3 pos;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
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
            new TargetingJob() { cWorld = cworld }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        /// <summary>
        /// all target buffer element must be clean up before any targeting job been perform
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(HideInClient))]
        public partial struct CleanUpTarget : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                DynamicBuffer<TargetBufferElement> targets)
            {
                targets.Clear();
            }
        }
        
        
        /// <summary>
        /// scan airborne target and cache them in weapon target buffer
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(HideInClient))]
        public partial struct TargetingJob : IJobEntity
        {
            [ReadOnly]
            public CollisionWorld cWorld;
            public void Execute(
                [EntityIndexInQuery] int index,
                DynamicBuffer<TargetBufferElement> targets,
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
                var hit = cWorld.OverlapBox(
                    ltw.Position,
                    default,
                    new float3(weapon.maxRange,25,weapon.maxRange),
                    ref outHits,
                    weapon.targetFilter);
                if (!hit) return;
                for (int i = 0,j=0; i < outHits.Length && j < weapon.targetCount; i++)
                {
                    var hitResult = outHits[i];
                    if (WithInRange(weapon,ltw.Position,ltw.Forward,hitResult.Position))
                    {
                        targets.Add(new TargetBufferElement(){value = hitResult.Entity,pos = hitResult.Position});
                        j++;
                    }
                }
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