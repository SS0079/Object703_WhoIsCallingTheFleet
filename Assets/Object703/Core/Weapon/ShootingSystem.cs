using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Authoring;
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
    public struct Weapon : IComponentData
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

            public Weapon ToComponentData(IBaker i)
            {
                var result = new Weapon();
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

    public struct ShootAtTick : IComponentData
    {
        [GhostField]public NetworkTick value;
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
    //                      .Query<RefRW<ShootAtTick>>().WithAll<Simulate>())
    //         {
    //             if (!tick.ValueRO.value.IsValid) tick.ValueRW.value = currentTick;
    //         }
    //     }
    // }
    //
    // [BurstCompile]
    // [RequireMatchingQueriesForUpdate]
    // [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    // [UpdateAfter(typeof(WeaponTargetingSystem))]
    // [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    // public partial struct ShootingSystem : ISystem
    // {
    //     [BurstCompile]
    //     public void OnCreate(ref SystemState state)
    //     {
    //         state.RequireForUpdate<NetworkTime>();
    //         // state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    //     }
    //
    //     [BurstCompile]
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         // var Δt = SystemAPI.Time.DeltaTime;
    //         var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
    //         Debug.Log(currentTick.TickValue);
    //         var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    //         foreach (var (targetBuffer,weapon,tick,ltw,random,owner) in SystemAPI.
    //                      Query<DynamicBuffer<TargetBufferElement>,
    //                          RefRW<Weapon>,
    //                          RefRW<ShootAtTick>,
    //                          RefRO<LocalToWorld>,
    //                          RefRW<IndividualRandom>,
    //                          RefRO<GhostOwner>>())
    //         {
    //             //skip if no target
    //             if(targetBuffer.Length==0) continue;
    //             //skip if tick is invlide
    //             if (!tick.ValueRO.value.IsValid) continue;
    //             //skip if time not reach
    //             if (currentTick.Equals(tick.ValueRO.value) || currentTick.IsNewerThan(tick.ValueRO.value))
    //             {
    //                 var targetPos = SystemAPI.GetComponentRO<LocalTransform>(targetBuffer[0].value);
    //                 for (int i = 0; i < weapon.ValueRO.salvo; i++)
    //                 {
    //                     SpawnCharge(state.EntityManager,weapon,ltw,targetPos.ValueRO.Position,random,owner);
    //                 }
    //                 weapon.ValueRW.burstCounter++;
    //                 var targetTick = currentTick;
    //                 if (weapon.ValueRO.burstCounter<weapon.ValueRO.burst)
    //                 {
    //                     //if burstCounter is smaller than burst, set next tick according to delayBetweenBurst
    //                     var waitTimeInTickF = weapon.ValueRO.delayBetweenBurst * simulationTickRate;
    //                     var timeInTick = (uint)waitTimeInTickF;
    //                     targetTick.Add(timeInTick);
    //                     tick.ValueRW.value = targetTick;
    //                 }
    //                 else
    //                 {
    //                     //if not, set timer to delayBetweenShot, and set burstCounter to 0
    //                     var waitTimeInTickF = weapon.ValueRO.delayBetweenShot * simulationTickRate;
    //                     var waitTimeInTick = (uint)waitTimeInTickF;
    //                     targetTick.Add(waitTimeInTick);
    //                     weapon.ValueRW.burstCounter = 0;
    //                 }
    //             }
    //             
    //         }
    //     }
    //
    //     private static Entity SpawnCharge(EntityManager entityManager,RefRW<Weapon> weapon,RefRO<LocalToWorld> ltw,float3 targetPos,RefRW<IndividualRandom> random,RefRO<GhostOwner> owner)
    //     {
    //         var localCharge = entityManager.Instantiate(weapon.ValueRO.charge);
    //         var muzzlePos = ltw.ValueRO.Position;
    //         var targetRot = quaternion.LookRotationSafe(targetPos - muzzlePos, ltw.ValueRO.Up);
    //         if (weapon.ValueRO.spread.x!=0 || weapon.ValueRO.spread.y!=0)
    //         {
    //             var halfX = weapon.ValueRO.spread.x / 2f;
    //             var xRot = math.radians(random.ValueRW.value.NextFloat(-halfX, halfX));
    //             var halfY = weapon.ValueRO.spread.y / 2f;
    //             var yRot = math.radians(random.ValueRW.value.NextFloat(-halfY, halfY));
    //             var spreadRot = quaternion.Euler(xRot, yRot, 0);
    //             targetRot = math.mul(targetRot, spreadRot);
    //         }
    //         entityManager.SetComponentData(localCharge,new LocalTransform{Position = muzzlePos,Rotation = targetRot,Scale = 1});
    //         entityManager.SetComponentData(localCharge,owner.ValueRO);
    //         return localCharge;
    //     }
    //
    //     [BurstCompile]
    //     public void OnDestroy(ref SystemState state)
    //     {
    //         
    //     }
    //
    // }
}