using System;
using KittyHelpYouOut;
using Object703.Core.VisualEffect;
using Object703.Core.Weapon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable Unity.Entities.MustBeSurroundedWithRefRwRo

namespace Object703.Authoring
{
    [Serializable]
    public class LineRendererActorPrefab : IComponentData, IEnableableComponent
    {
        public GameObject prefab;
    }
    
    [Serializable]
    public class LineRendererActor : ICleanupComponentData
    {
        [FormerlySerializedAs("go")]
        public LineRenderer value;
    }
    
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public class GameObjectActor : ICleanupComponentData
    {
        public GameObject actor;
    }
    
    [Serializable]
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct AttachGameObject : IComponentData
    {
        public FixedString32Bytes prefabName;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnGameObjectActorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            {
                //add game object actor clean up component data to those who have prefab component but have no actor component
                var unspawnedGoQuery = SystemAPI.QueryBuilder().WithAll<AttachGameObject>().WithNone<GameObjectActor>().Build().ToEntityArray(state.WorldUpdateAllocator);
                for (int i = 0, count = unspawnedGoQuery.Length; i < count; i++)
                {
                    state.EntityManager.AddComponentObject(unspawnedGoQuery[i], new GameObjectActor());
                }
                var prefabConfig = SystemAPI.QueryBuilder().WithAll<GameObjectPrefabConfig>().Build().GetSingleton<GameObjectPrefabConfig>();

                //spawn game object actor for entities who have game object actor prefab
                //this foreach spawn for local player
                foreach (var (prefabName, actor) in SystemAPI
                             .Query<RefRO<AttachGameObject>, GameObjectActor>().WithAll<GhostOwnerIsLocal>())
                {
                    var localKey = prefabName.ValueRO.prefabName.ToString()+"_Local";
                    var exist = prefabConfig.prefabDic.TryGetValue(localKey,out GameObject prefab);
                    if(!exist) break;
                    var go = prefab.GetPoolObject();
                    actor.actor = go;
                }
                
                //this foreach spawn for remote player
                foreach (var (prefabName, actor) in SystemAPI
                             .Query<RefRO<AttachGameObject>, GameObjectActor>().WithNone<GhostOwnerIsLocal>())
                {
                    var localKey = prefabName.ValueRO.prefabName.ToString()+"_Remote";
                    var exist = prefabConfig.prefabDic.TryGetValue(localKey,out GameObject prefab);
                    if(!exist) break;
                    var go = prefab.GetPoolObject();
                    actor.actor = go;
                }

                //turn off prefab component after spawn
                var spawnedQuery = SystemAPI.QueryBuilder().WithAll<AttachGameObject, GameObjectActor>().Build();
                state.EntityManager.RemoveComponent<AttachGameObject>(spawnedQuery);
                
                //recycle unused actor
                foreach (var gameObjectActor in SystemAPI.Query<GameObjectActor>().WithNone<LocalTransform>())
                {
                    Debug.Log($"Cleaning");
                    gameObjectActor.actor.gameObject.SetActive(false);
                }
                var cleanUpQuery = SystemAPI.QueryBuilder().WithAll<GameObjectActor>().WithNone<LocalTransform>().Build();
                state.EntityManager.RemoveComponent<GameObjectActor>(cleanUpQuery);
            }

            {
                //add lineRendererActor cleanUp component data to those who have prefab component but have no actor component
                var unspawnedLineQuery = SystemAPI.QueryBuilder().WithAll<LineRendererActorPrefab,GhostOwnerIsLocal>().WithNone<LineRendererActor>().Build()
                    .ToEntityArray(state.WorldUpdateAllocator);
                for (int i = 0; i < unspawnedLineQuery.Length; i++)
                {
                    state.EntityManager.AddComponentObject(unspawnedLineQuery[i], new LineRendererActor());
                }

                //spawn lineRenderer actor for entities who have lineRenderer prefab
                foreach (var (prefab, actor) in SystemAPI.Query<LineRendererActorPrefab, LineRendererActor>())
                {
                    var line = prefab.prefab.gameObject.GetPoolObject();
                    line.transform.forward = Vector3.up;
                    actor.value = line.GetComponent<LineRenderer>();
                }
                var spawnedQuery = SystemAPI.QueryBuilder().WithAll<LineRendererActorPrefab, LineRendererActor>().Build();
                state.EntityManager.SetComponentEnabled<LineRendererActorPrefab>(spawnedQuery,false);
                
                //recycle unused line
                foreach (var line in SystemAPI.Query<LineRendererActor>().WithNone<LocalTransform>())
                {
                    Debug.Log($"Cleaning");
                    line.value.gameObject.SetActive(false);
                }
                var cleanUpQuery = SystemAPI.QueryBuilder().WithAll<LineRendererActor>().WithNone<LocalTransform>().Build();
                state.EntityManager.RemoveComponent<LineRendererActor>(cleanUpQuery);
            }
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}