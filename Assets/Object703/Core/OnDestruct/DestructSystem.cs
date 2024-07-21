using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Core
{
    #region componentData
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
    public struct DestructTag : IComponentData , IEnableableComponent
    {
    }
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct HideInClientTag : IComponentData , IEnableableComponent
    {
        // public float3 lastPosition;
        // public quaternion lastRotation;
        // public float lastScale;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct DestructEffectPrefabs : IComponentData
    {
        public Entity value0;
        public Entity value1;
        public Entity value2;
        public Entity value3;
        
        public void Spawn(EntityManager manager,LocalTransform trans)
        {
            if (value0!=Entity.Null)
            {
                var e = manager.Instantiate(value0);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
            if (value1!=Entity.Null)
            {
                var e = manager.Instantiate(value1);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
            if (value2!=Entity.Null)
            {
                var e = manager.Instantiate(value2);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
            if (value3!=Entity.Null)
            {
                var e = manager.Instantiate(value3);
                manager.SetComponentData(e, new LocalPositionInitializer() { position = trans.Position });
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct LocalPositionInitializer : IComponentData , IEnableableComponent
    {
        public float3 position;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct LocalRotationInitializer : IComponentData , IEnableableComponent
    {
        public quaternion rotation;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct LocalScaleInitializer : IComponentData , IEnableableComponent
    {
        public float3 scale;
    }
    
    public struct DestructSpawnPrefabs : IComponentData
    {
        public Entity value0;
        public Entity value1;
        public Entity value2;
        public Entity value3;

        public void Spawn(EntityManager manager,LocalTransform trans,GhostOwner owner)
        {
            var newTrans = LocalTransform.FromPositionRotation(trans.Position,trans.Rotation);
            if (value0!=Entity.Null)
            {
                var e = manager.Instantiate(value0);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
            if (value1!=Entity.Null)
            {
                var e = manager.Instantiate(value1);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
            if (value2!=Entity.Null)
            {
                var e = manager.Instantiate(value2);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
            if (value3!=Entity.Null)
            {
                var e = manager.Instantiate(value3);
                manager.SetComponentData(e,newTrans);
                manager.SetComponentData(e,owner);
            }
        }
    }
    public struct CanDestructGhost : ICommandData
    {
        [GhostField]
        public NetworkTick Tick { get; set; }
        [GhostField]
        public InputEvent destruct;
    }
    #endregion
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnDestrcutSystemGroup))]
    public partial struct SelfDestructTickSystem : ISystem
    {
        private float3 hideOutPos;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

            //check if current tick reach the destruct tick
            foreach (var (destructTick,destructEn) in SystemAPI.Query<DynamicBuffer<SelfDestructAtTick>,EnabledRefRW<DestructTag>>().WithAll<Simulate>().WithDisabled<DestructTag>())
            {
                destructTick.GetDataAtTick(currentTick, out var localTick);
                if(localTick.Tick==NetworkTick.Invalid || localTick.value==NetworkTick.Invalid) continue;
                if (currentTick.Equals(localTick.value) || currentTick.IsNewerThan(localTick.value))
                {
                    destructEn.ValueRW = true;
                }
            }
        }
    }
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AfterDestructSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct DestructGhostClientSystem : ISystem
    {
        private float3 hideOutPos;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            hideOutPos = new float3(10000, 10000, 10000);
        }

        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (networkTime.IsFirstTimeFullyPredictingTick) return;
            
            // spawn entity if have entity to spawn on destruct
            foreach (var (prefabs,trans,owner) in SystemAPI
                         .Query<RefRW<DestructSpawnPrefabs>,RefRO<LocalTransform>,RefRO<GhostOwner>>().WithAll<Simulate,DestructTag>().WithNone<HideInClientTag>())
            {
                prefabs.ValueRW.Spawn(state.EntityManager,trans.ValueRO,owner.ValueRO);
            }

            // spawn effect if have effect to spawn on destruct
            foreach (var (prefabs,trans) in SystemAPI
                         .Query<RefRW<DestructEffectPrefabs>,RefRO<LocalTransform>>().WithAll<Simulate,DestructTag>().WithNone<HideInClientTag>())
            {
                prefabs.ValueRW.Spawn(state.EntityManager,trans.ValueRO);
            }
            
            //sync position for new spawn effect
            foreach (var (positionProxy,trans) in SystemAPI
                         .Query<RefRO<LocalPositionInitializer>,RefRW<LocalTransform>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                trans.ValueRW.Position = positionProxy.ValueRO.position;
            }

            //disable all local position proxy, those should have been handled
            foreach (var enPosProxy in SystemAPI
                         .Query<EnabledRefRW<LocalPositionInitializer>>().WithAll<Simulate>().WithNone<DestructTag>())
            {
                enPosProxy.ValueRW = false;
            }
            
            // set hitInClientTag to active if entity have destruct tag
            foreach (var enHide in SystemAPI
                         .Query<EnabledRefRW<HideInClientTag>>().WithAll<Simulate,DestructTag>().WithDisabled<HideInClientTag>())
            {
                enHide.ValueRW = true;
            }
            
            
            
            //hide ghost if this is client world
            foreach (var trans in SystemAPI
                         .Query<RefRW<LocalTransform>>()
                         .WithAll<DestructTag,GhostInstance>())
            {
                trans.ValueRW.Position = hideOutPos;
            }
        }
    }
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(OnDestrcutSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DestructGhostServerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // spawn entity if have entity to spawn on destruct
            foreach (var (prefabs,trans,owner) in SystemAPI
                         .Query<RefRW<DestructSpawnPrefabs>,RefRO<LocalTransform>,RefRO<GhostOwner>>().WithAll<Simulate,DestructTag>())
            {
                prefabs.ValueRW.Spawn(state.EntityManager,trans.ValueRO,owner.ValueRO);
            }
            
            //destruct immediately if this is server world
            var destructQuery = SystemAPI.QueryBuilder().WithAll<DestructTag,GhostInstance>().Build().ToEntityArray(state.WorldUpdateAllocator);
            state.EntityManager.DestroyEntity(destructQuery);
        }
    }
    
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AfterDestructSystemGroup))]
    public partial struct DestructNonGhostSystem : ISystem
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
            foreach (var (timer,destructEn) in SystemAPI
                         .Query<RefRW<LifeSpanSecond>,
                             EnabledRefRW<DestructTag>>()
                         .WithAll<Simulate>().WithDisabled<DestructTag>()
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
            //destruct local entity but leave ghost entity alone
            var clientDestructQuery = SystemAPI.QueryBuilder().WithAll<DestructTag>().WithNone<GhostInstance,SelfDestructAtTick>().Build().ToEntityArray(state.WorldUpdateAllocator);
            state.EntityManager.DestroyEntity(clientDestructQuery);
        }
    }
}