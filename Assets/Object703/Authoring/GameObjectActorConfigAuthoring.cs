using System.Collections.Generic;
using KittyDOTS;
using Unity.Entities;
using UnityEngine;

namespace Object703.Core.VisualEffect
{
    public class GameObjectPrefabConfig : IComponentData
    {
        public Dictionary<string,GameObject> prefabDic;
    }
    [DisallowMultipleComponent]
    public class GameObjectActorConfigAuthoring : MonoBehaviour
    {
        public GameObject[] prefabs;
        class GameObjectConfigBaker : Baker<GameObjectActorConfigAuthoring>
        {
            public override void Bake(GameObjectActorConfigAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                if (authoring.prefabs.Length>0)
                {
                    var gameObjectPrefab = new GameObjectPrefabConfig();
                    gameObjectPrefab.prefabDic = new Dictionary<string, GameObject>(16);
                    for (int i = 0; i < authoring.prefabs.Length; i++)
                    {
                        var item = authoring.prefabs[i];
                        gameObjectPrefab.prefabDic.TryAdd(item.name, item);
                    }
                    AddComponentObject(self,gameObjectPrefab);
                }
            }
        }
    }
}