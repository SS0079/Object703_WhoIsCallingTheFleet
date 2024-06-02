using KittyHelpYouOut;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Object703.Core.NetCode
{
    public class FrontendConnectionManager : KittyMonoSingletonManual<FrontendConnectionManager>
    {
        private string oldFrontendWorldName;

        public void StartClientServer(ushort port)
        {
            if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
            {
                Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
                return;
            }
            var clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            var serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            DisposeLocalWorld();
            World.DefaultGameObjectInjectionWorld = serverWorld;
            SceneManager.LoadSceneAsync("SampleScene");
            NetworkEndpoint nep = NetworkEndpoint.AnyIpv4.WithPort(port);
            {
                using var entityQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                entityQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(nep);
            }
            nep = NetworkEndpoint.LoopbackIpv4.WithPort(port);
            {
                using var entityQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                entityQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager,nep);
            }
        }

        public void ConnectToServer(string ip,ushort port)
        {
            if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.Server)
            {
                Debug.LogError($"Connect to server is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
                return;
            }
            var clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            DisposeLocalWorld();
            World.DefaultGameObjectInjectionWorld = clientWorld;
            SceneManager.LoadSceneAsync("SampleScene");
            var nep = NetworkEndpoint.Parse(ip,port);
            var entityQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            entityQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager,nep);
        }

        private void DisposeLocalWorld()
        {
            foreach (var item in World.All)
            {
                if (item.Flags==WorldFlags.Game)
                {
                    oldFrontendWorldName = item.Name;
                    item.Dispose();
                    break;
                }
            }
        }
    }
}