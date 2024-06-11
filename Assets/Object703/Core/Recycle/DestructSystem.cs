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

    public struct SelfDestructAtTick : IComponentData , IEnableableComponent
    {
        [GhostField]public NetworkTick value;
    }
    
    [Serializable]
    [GhostEnabledBit]
    public struct DestructTag : IComponentData , IEnableableComponent
    {
    }
    
    public partial struct PrepareDestructTimerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            var serverTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            //calculate the target net tick of target timer
            foreach (var (timer,tick) in SystemAPI.Query<RefRO<LifeSpanTick>,RefRW<SelfDestructAtTick>>().WithAll<Simulate>().WithDisabled<SelfDestructAtTick>())
            {
                var lifeTimeInTick = (uint)(timer.ValueRO.value*simulationTickRate);
                var targetTick = serverTick;
                targetTick.Add(lifeTimeInTick);
                tick.ValueRW.value = targetTick;
            }
            //set destruct time tick to enable to avoid repeat tick setting
            foreach (var enableTick in SystemAPI.Query<EnabledRefRW<SelfDestructAtTick>>().WithAll<Simulate,LifeSpanTick>().WithDisabled<SelfDestructAtTick>())
            {
                enableTick.ValueRW = true;
            }
        }
    }
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct PredicatedDestructSystem : ISystem
    {
        private float3 hideOutPos;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            hideOutPos = new float3(0, 2000, 0);
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            //check if current tick reach the destruct tick
            foreach (var (destructTick,destructEn) in SystemAPI.Query<RefRO<SelfDestructAtTick>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithDisabled<DestructTag>())
            {
                if (currentTick.Equals(destructTick.ValueRO.value) || currentTick.IsNewerThan(destructTick.ValueRO.value))
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
            foreach (var (timer,destructEn) in SystemAPI.Query<RefRW<LifeSpanTick>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithDisabled<DestructTag>()
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
            foreach (var item in clientDestructQuery)
            {
                
            }
            state.EntityManager.DestroyEntity(clientDestructQuery);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        
    }

}