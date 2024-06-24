using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.Serialization;

namespace Object703.Core.Recycle
{
    [Serializable]
    public struct LifeSpanTick : IComponentData
    {
        [FormerlySerializedAs("tick")]
        [GhostField]public uint value;
    }

    public struct LifeSpanSecond : IComponentData ,IEnableableComponent
    {
        public float value;
    }

    public struct SelfDestructPrepared : IComponentData , IEnableableComponent
    {
        
    }
    public struct SelfDestructAtTick : ICommandData
    {
        [GhostField]public NetworkTick Tick { get; set; }
        [GhostField]public NetworkTick value;
    }
    
    [Serializable]
    [GhostEnabledBit]
    public struct DestructTag : IComponentData , IEnableableComponent
    {
    }
    
    
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct DestructSystem : ISystem
    {
        private float3 hideOutPos;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            hideOutPos = new float3(10000, 10000, 10000);
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            
            //check if current tick reach the destruct tick
            foreach (var (destructTick,destructEn) in SystemAPI.Query<DynamicBuffer<SelfDestructAtTick>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithDisabled<DestructTag>())
            {
                destructTick.GetDataAtTick(currentTick, out var localTick);
                if(localTick.value==NetworkTick.Invalid) continue;
                if (currentTick.Equals(localTick.value) || currentTick.IsNewerThan(localTick.value))
                {
                    destructEn.ValueRW = true;
                }
            }
            if (state.World.Flags==WorldFlags.GameServer)
            {
                //destruct immediately if this is server world
                var destructQuery = SystemAPI.QueryBuilder().WithAll<DestructTag,GhostInstance>().Build().ToEntityArray(state.WorldUpdateAllocator);
                state.EntityManager.DestroyEntity(destructQuery);
            }
            if (state.World.Flags==WorldFlags.GameClient || state.World.Flags==WorldFlags.GameThinClient)
            {
                //hide ghost if this is client world
                foreach (var (trans,simulateEn) in SystemAPI
                             .Query<RefRW<LocalTransform>,EnabledRefRW<Simulate>>().WithAll<Simulate,DestructTag,GhostInstance>())
                {
                    trans.ValueRW.Position = hideOutPos;
                    simulateEn.ValueRW = false;
                }
            }
            
        }
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup),OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DestructClientSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var Δt = SystemAPI.Time.DeltaTime;
            //count down self-destruct timer
            foreach (var (timer,destructEn) in SystemAPI.Query<RefRW<LifeSpanSecond>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithDisabled<DestructTag>()
                    .WithNone<SelfDestructAtTick>())
            {
                if (timer.ValueRO.value<=0)
                {
                    destructEn.ValueRW=true;
                }
                else
                {
                    timer.ValueRW.value -= Δt;
                }
            }
            //destruct local entity but leave network entity alone
            var clientDestructQuery = SystemAPI.QueryBuilder().WithAll<DestructTag>().WithNone<GhostInstance,SelfDestructAtTick>().Build().ToEntityArray(state.WorldUpdateAllocator);
            state.EntityManager.DestroyEntity(clientDestructQuery);
        }
    }
}