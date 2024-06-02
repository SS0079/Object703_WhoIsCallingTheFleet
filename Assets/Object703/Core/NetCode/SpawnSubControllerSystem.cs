using Object703.Authoring;
using Object703.Core.Control;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core.NetCode
{
    public struct GhostParent : IComponentData
    {
        [GhostField]
        public Entity parent;
    }
    
    public struct WeaponAndSkillPrefab : IBufferElementData
    {
        public FixedString32Bytes name;
        public Entity value;
    }
    
    public readonly partial struct WeaponAndSkillSpawnAspect : IAspect
    {
        public readonly DynamicBuffer<WeaponAndSkillPrefab> prefabs;

        public bool FindByName(FixedString32Bytes name, out Entity result)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i].name.Equals(name))
                {
                    result = prefabs[i].value;
                    return true;
                }
            }
            result = Entity.Null;
            return false;
        }
    }

    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnSubControllerClientSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<NetworkId>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (ghostParent,childEntity) in SystemAPI.Query<RefRW<GhostParent>>().WithEntityAccess())
            {
                ecb.AddComponent(childEntity,new Parent(){Value = ghostParent.ValueRO.parent});
                ecb.RemoveComponent<GhostParent>(childEntity);
                ecb.AppendToBuffer(ghostParent.ValueRO.parent,new LinkedEntityGroup(){Value = childEntity});
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
    public partial struct SpawnSubControllerServerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeaponAndSkillPrefab>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var weaponSkillPrefab = SystemAPI.GetSingletonEntity<WeaponAndSkillPrefab>();
            var weaponSkillAspect = SystemAPI.GetAspect<WeaponAndSkillSpawnAspect>(weaponSkillPrefab);
            foreach (var (spawn,owner,playerEntity) in SystemAPI
                         .Query<RefRO<SpawnSubControllerName>,RefRO<GhostOwner>>().WithEntityAccess())
            {
                //spawn sub controller for player in server
                var spawnString = spawn.ValueRO.value.ToString().Split('|');
                for (int i = 0; i < spawnString.Length; i++)
                {
                    if (weaponSkillAspect.FindByName(new FixedString32Bytes(spawnString[i]), out Entity result))
                    {
                        var localController = ecb.Instantiate(result);
                        ecb.SetComponent(localController,new GhostParent(){parent = playerEntity});
                        ecb.AddComponent(localController,new Parent(){Value = playerEntity});
                        ecb.SetComponent(localController,owner.ValueRO);
                        ecb.AppendToBuffer(playerEntity,new LinkedEntityGroup(){Value = localController});
                    }
                }
                ecb.RemoveComponent<SpawnSubControllerName>(playerEntity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}