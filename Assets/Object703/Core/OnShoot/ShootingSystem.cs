using System;
using KittyHelpYouOut;
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

namespace Object703.Core
{
     [Flags]
    public enum ColliderLayers
    {
        Caster=1<<0,
        Terran=1<<1,
        SurfacePlayer=1<<2,
        AirbornePlayer=1<<3,
        SurfaceAlly=1<<10,
        AirborneAlly=1<<11,
        SurfaceEnemy=1<<20,
        AirborneEnemy=1<<21,
        Neutral=1<<25
    }

    public static class ColliderLayersExtension
    {
        public static uint ToUInt(this ColliderLayers layers) => (uint)layers;
    }
    [Serializable]
    public struct Weapon_FanRange : IComponentData
    {
        public float maxRange;
        public float minRange;
        public float2 spread;
        public int salvo;
        public int burst;
        public int burstCounter;
        public CollisionFilter targetFilter;
        public Entity charge;
        public float delayBetweenShot;
        public float delayBetweenBurst;
        public int targetCount;
        public byte targetBufferHead,targetBufferTail; 
        [SerializeField]
        private float _arcLimit;
        public float ArcLimit
        {
            get => _arcLimit;
            set
            {
                _arcLimit = value;
                arcLimitCos = math.cos(math.radians(_arcLimit) / 2f);
            }
        }
        private float arcLimitCos;
        public float ArcLimitCos => arcLimitCos;
        
        public bool FindTarget(LocalToWorld muzzleltw,CollisionWorld cWorld,ComponentLookup<LocalTransform> transLp,ref NativeList<Entity> targetList)
        {
            // TODO: improve aabb calculation later on
            // // calculate range fan left and right far point
            // var fanFarRight = math.mul(quaternion.RotateY(math.radians(ArcLimit / 2f)), new float3(0, 0, maxRange));
            // fanFarRight = muzzleTrans.InverseTransformPoint(fanFarRight);
            // var fanFarLeft = math.mul(quaternion.RotateY(math.radians(-ArcLimit / 2f)), new float3(0, 0, maxRange));
            // fanFarLeft = muzzleTrans.InverseTransformPoint(fanFarLeft);
            // if (ArcLimit<180)
            // {
            //     //find aabb in fanFarRight, fanFarLeft, muzzle, centerFar
            // }
            targetList.Clear();
            var result = false;
            var muzzlePos = muzzleltw.Position;
            var min = new float3(muzzlePos.x - maxRange, -25, muzzlePos.z - maxRange);
            var max = new float3(muzzlePos.x + maxRange, 25, muzzlePos.z + maxRange);
            OverlapAabbInput input = new OverlapAabbInput
            {
                Aabb = new Aabb
                {
                    Min = min,
                    Max = max
                },
                Filter = targetFilter
            };
            NativeList<int> allhit = new NativeList<int>(Allocator.TempJob);
            var isHit = cWorld.OverlapAabb(input,ref allhit);
            if (isHit)
            {
                for (int i = 0; i < allhit.Length; i++)
                {
                    var hitEntity = cWorld.Bodies[allhit[i]].Entity;
                    var entityPos = transLp[hitEntity].Position;
                    var disSq = math.distancesq(muzzlePos, entityPos);
                    // continue if too far or too close
                    if (disSq<minRange*minRange || disSq>maxRange*maxRange) continue;
                    entityPos = new float3(entityPos.x, 0, entityPos.z);
                    muzzlePos = new float3(muzzlePos.x, 0, muzzlePos.z);
                    var muzzleFwd = new float3(muzzleltw.Forward.x, 0, muzzleltw.Forward.z);
                    var targetDir = math.normalizesafe(entityPos - muzzlePos);
                    var dot = math.dot(muzzleFwd, targetDir);
                    //continue false if not in arc
                    if (dot<ArcLimitCos) continue;
                    targetList.Add(hitEntity);
                    result = true;
                }
            }
            return result;
        }
        
        [Serializable]
        public struct AuthoringBox
        {
            [FormerlySerializedAs("Range")]
            public float maxRange;
            public float minRange;
            public float2 spread;
            public int salvo;
            public int burst;
            [FormerlySerializedAs("TargetFilter")]
            public PhysicsMaterialTemplate targetFilter;
            [FormerlySerializedAs("Charge")]
            public GameObject charge;
            [FormerlySerializedAs("DelayBetweenShot")]
            public float delayBetweenShot;
            public float delayBetweenBurst;
            [FormerlySerializedAs("TargetCount")]
            public int targetCount;
            public float arcLimit;

            public Weapon_FanRange ToComponentData(IBaker i)
            {
                var result = new Weapon_FanRange();
                result.maxRange = maxRange;
                result.minRange = minRange;
                result.spread = spread;
                result.salvo = salvo;
                result.burst = burst;
                result.targetFilter = new CollisionFilter
                {
                    BelongsTo = targetFilter.BelongsTo.Value,
                    CollidesWith = targetFilter.CollidesWith.Value,
                    GroupIndex = targetFilter.CustomTags.Value
                };
                result.charge = i.GetEntity(charge,TransformUsageFlags.Dynamic);
                result.targetCount = targetCount;
                result.delayBetweenShot = delayBetweenShot;
                result.delayBetweenBurst = delayBetweenBurst;
                result.ArcLimit = arcLimit;
                return result;
            }
        }
    }

    public struct ShootAtTick : ICommandData
    {
        [GhostField]public NetworkTick Tick { get; set; }
        [GhostField]public NetworkTick coolDownAtTick;
    }
    // [RequireMatchingQueriesForUpdate]
    // [UpdateInGroup(typeof(SimulationSystemGroup))]
    // [UpdateBefore(typeof(PredictedSimulationSystemGroup))]
    // public partial struct InitWeaponSystem : ISystem
    // {
    //     public void OnCreate(ref SystemState state)
    //     {
    //         state.RequireForUpdate<NetworkTime>();
    //     }
    //
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
    //         foreach (var tick in SystemAPI
    //                      .Query<DynamicBuffer<ShootAtTick>>().WithAll<Simulate>())
    //         {
    //             if (!tick.ValueRO.value.IsValid) tick.ValueRW.value = currentTick;
    //         }
    //     }
    // }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnPredicatedShootSystemGroup))]
    public partial struct ShootingSystem : ISystem
    {
        private int simulationTickRate;
        private ComponentLookup<LocalTransform> localTransLp;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<NetworkTime>();
            simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(true);
        }
    
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = networkTime.ServerTick;
            if (!networkTime.IsFirstTimeFullyPredictingTick) return;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            localTransLp.Update(ref state);
            foreach (var (weapon,tick,ltw,random,owner) in SystemAPI.
                         Query<RefRW<Weapon_FanRange>,
                             DynamicBuffer<ShootAtTick>,
                             RefRO<LocalToWorld>,
                             RefRW<IndividualRandom>,
                             RefRO<GhostOwner>>())
            {
                var onTime = false;
                for (uint i = 0u; i < networkTime.SimulationStepBatchSize; i++)
                {
                    var localNow = currentTick.SubSpan(i);
                    if (!tick.GetDataAtTick(localNow,out var localTick))
                    {
                        localTick.coolDownAtTick = NetworkTick.Invalid;
                    }
                    if (localTick.coolDownAtTick == NetworkTick.Invalid || localNow.IsNewerThan(localTick.coolDownAtTick))
                    {
                        onTime = true;
                    }
                }
                //skip if time not reach
                if (onTime)
                {
                    NativeList<Entity> targetList = new NativeList<Entity>(Allocator.TempJob);
                    var hasTarget = weapon.ValueRW.FindTarget(ltw.ValueRO,collisionWorld,localTransLp,ref targetList);
                    if (hasTarget)
                    {
                        var targetPos = localTransLp[targetList[0]];
                        for (int i = 0; i < weapon.ValueRO.salvo; i++)
                        {
                            SpawnCharge(state.EntityManager,weapon,ltw,targetPos.Position,random,owner);
                        }
                        weapon.ValueRW.burstCounter++;
                        var waitInTick = 0u;
          
                        if (weapon.ValueRO.burstCounter<weapon.ValueRO.burst)
                        {
                            //if burstCounter is smaller than burst, set next tick according to delayBetweenBurst
                            waitInTick =(uint)(weapon.ValueRO.delayBetweenBurst * simulationTickRate);
                        }
                        else
                        {
                            //if not, set timer to delayBetweenShot, and set burstCounter to 0
                            waitInTick = (uint)(weapon.ValueRO.delayBetweenShot * simulationTickRate);
                            weapon.ValueRW.burstCounter = 0;
                        }
                        if (state.WorldUnmanaged.IsServer()) continue;
                        tick.AddCommandData(new ShootAtTick(){Tick = currentTick.AddSpan(1u),coolDownAtTick = currentTick.AddSpan(waitInTick)});
                    }
                }
                
            }
        }
    
        private static Entity SpawnCharge(EntityManager entityManager,RefRW<Weapon_FanRange> weapon,RefRO<LocalToWorld> ltw,float3 targetPos,RefRW<IndividualRandom> random,
            RefRO<GhostOwner> owner)
        {
            var localCharge = entityManager.Instantiate(weapon.ValueRO.charge);
            var muzzlePos = ltw.ValueRO.Position;
            var targetRot = quaternion.LookRotationSafe(targetPos - muzzlePos, ltw.ValueRO.Up);
            if (weapon.ValueRO.spread.x!=0 || weapon.ValueRO.spread.y!=0)
            {
                var halfX = weapon.ValueRO.spread.x / 2f;
                var xRot = math.radians(random.ValueRW.value.NextFloat(-halfX, halfX));
                var halfY = weapon.ValueRO.spread.y / 2f;
                var yRot = math.radians(random.ValueRW.value.NextFloat(-halfY, halfY));
                var spreadRot = quaternion.Euler(xRot, yRot, 0);
                targetRot = math.mul(targetRot, spreadRot);
            }
            entityManager.SetComponentData(localCharge,new LocalTransform{Position = muzzlePos,Rotation = targetRot,Scale = 1});
            entityManager.SetComponentData(localCharge,owner.ValueRO);
            return localCharge;
        }
    }
}