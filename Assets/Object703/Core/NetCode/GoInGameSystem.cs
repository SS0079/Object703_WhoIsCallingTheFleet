using Object703.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Object703.Core.NetCode
{
    public struct GoInGameRequest : IRpcCommand
    {
        
    }
    
    public struct PlayerSpawner : IComponentData
    {
        public Entity prefab;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct GoInGameClientSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EnableGameTag>();
            state.RequireForUpdate<PlayerSpawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (networkId,connectionEntity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                ecb.AddComponent<NetworkStreamInGame>(connectionEntity);
                var requestEntity = state.EntityManager.CreateEntity();
                ecb.AddComponent<GoInGameRequest>(requestEntity);
                ecb.AddComponent(requestEntity,new SendRpcCommandRequest(){TargetConnection = connectionEntity});
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GoInGameServerSystem : ISystem
    {
        private ComponentLookup<NetworkId> networkIdLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerSpawner>();
            networkIdLp = SystemAPI.GetComponentLookup<NetworkId>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var prefab = SystemAPI.GetSingleton<PlayerSpawner>().prefab;
            networkIdLp.Update(ref state);
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (rpcReq,reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequest>().WithEntityAccess())
            {
                var connectionEntity = rpcReq.ValueRO.SourceConnection;
                var id = networkIdLp[connectionEntity].Value;
                var player = ecb.Instantiate(prefab);
                ecb.SetComponent(player,new GhostOwner(){NetworkId = id});
                ecb.AppendToBuffer(connectionEntity,new LinkedEntityGroup(){Value = player});
                ecb.AddComponent<NetworkStreamInGame>(connectionEntity);
                ecb.DestroyEntity(reqEntity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}