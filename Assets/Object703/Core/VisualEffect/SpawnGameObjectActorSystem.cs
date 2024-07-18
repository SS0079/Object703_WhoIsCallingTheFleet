using System;
using KittyHelpYouOut;
using Object703.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable Unity.Entities.MustBeSurroundedWithRefRwRo

namespace Object703.Core
{
    [Serializable]
    public class AttachLineRenderer : IComponentData, IEnableableComponent
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
    public struct GameObjectActor : ICleanupComponentData
    {
        [SerializeField]
        private UnityObjectRef<GameObject> actor;
        public GameObject Get()
        {
            if (actor.Value==null)
            {
                Debug.Log($"Null happend");
            }
            return actor.Value;
        }

        public void Set(GameObject value)
        {
            if (value==null)
            {
                Debug.Log($"Null happend");
            }
            actor.Value = value;
        }
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
    public partial class SpawnGameObjectActorSystem : SystemBase
    {
        // private Dictionary<string, GameObject> prefabDic;
        protected override void OnUpdate()
        {
            if (AssetLoaderManager.Instance==null) return;
            var prefabDic = AssetLoaderManager.Instance.prefabDic;
            {
                //add game object actor clean up component data to those who have prefab component but have no actor component
                var unspawnedGoQuery = SystemAPI.QueryBuilder().WithAll<AttachGameObject>().WithNone<GameObjectActor>().Build().ToEntityArray(this.WorldUpdateAllocator);
                for (int i = 0, count = unspawnedGoQuery.Length; i < count; i++)
                {
                    EntityManager.AddComponentData(unspawnedGoQuery[i], new GameObjectActor());
                }

                //spawn game object actor for entities who have game object actor prefab
                //this foreach spawn for local player
                foreach (var (prefabName, actor) in SystemAPI
                             .Query<RefRO<AttachGameObject>, RefRW<GameObjectActor>>().WithAll<GhostOwnerIsLocal>().WithNone<DestructTag>())
                {
                    var localKey = prefabName.ValueRO.prefabName.ToString()+"_Local";
                    var exist = prefabDic.TryGetValue(localKey,out GameObject prefab);
                    if(!exist) break;
                    var go = prefab.GetPoolObject();
                    actor.ValueRW.Set(go);
                }
                
                //this foreach spawn for remote player
                foreach (var (prefabName, actor) in SystemAPI
                             .Query<RefRO<AttachGameObject>, RefRW<GameObjectActor>>().WithNone<GhostOwnerIsLocal>().WithNone<DestructTag>())
                {
                    var localKey = prefabName.ValueRO.prefabName.ToString()+"_Remote";
                    var exist = prefabDic.TryGetValue(localKey,out GameObject prefab);
                    if(!exist) break;
                    var go = prefab.GetPoolObject();
                    actor.ValueRW.Set(go);
                }

                //turn off prefab component after spawn
                var spawnedQuery = SystemAPI.QueryBuilder().WithAll<AttachGameObject, GameObjectActor>().Build();
                EntityManager.RemoveComponent<AttachGameObject>(spawnedQuery);
                
                //recycle unused actor
                foreach (var (gameObjectActor,entity) in SystemAPI.Query<RefRW<GameObjectActor>>().WithNone<LocalTransform>().WithEntityAccess())
                {
                    Debug.Log($"Cleaning");
                    if (gameObjectActor.ValueRW.Get()==null) continue;
                    gameObjectActor.ValueRW.Get().RecyclePoolObject();
                }
                var cleanUpQuery = SystemAPI.QueryBuilder().WithAll<GameObjectActor>().WithNone<LocalTransform>().Build();
                EntityManager.RemoveComponent<GameObjectActor>(cleanUpQuery);
            }

            {
                //add lineRendererActor cleanUp component data to those who have prefab component but have no actor component
                var unspawnedLineQuery = SystemAPI.QueryBuilder().WithAll<AttachLineRenderer,GhostOwnerIsLocal>().WithNone<LineRendererActor>().Build()
                    .ToEntityArray(this.WorldUpdateAllocator);
                for (int i = 0; i < unspawnedLineQuery.Length; i++)
                {
                    EntityManager.AddComponentObject(unspawnedLineQuery[i], new LineRendererActor());
                }

                //spawn lineRenderer actor for entities who have lineRenderer prefab
                foreach (var (prefab, actor) in SystemAPI.Query<AttachLineRenderer, LineRendererActor>().WithNone<DestructTag>())
                {
                    var line = prefab.prefab.gameObject.GetPoolObject();
                    line.transform.forward = Vector3.up;
                    actor.value = line.GetComponent<LineRenderer>();
                }
                var spawnedQuery = SystemAPI.QueryBuilder().WithAll<AttachLineRenderer, LineRendererActor>().Build();
                EntityManager.SetComponentEnabled<AttachLineRenderer>(spawnedQuery,false);
                
                //recycle unused line
                foreach (var line in SystemAPI.Query<LineRendererActor>().WithNone<LocalTransform>())
                {
                    Debug.Log($"Cleaning");
                    line.value.gameObject.RecyclePoolObject();
                }
                var cleanUpQuery = SystemAPI.QueryBuilder().WithAll<LineRendererActor>().WithNone<LocalTransform>().Build();
                EntityManager.RemoveComponent<LineRendererActor>(cleanUpQuery);
            }
        }
    }
}