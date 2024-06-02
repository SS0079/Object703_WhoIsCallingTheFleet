using Object703.Core.NetCode;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{

    
    [DisallowMultipleComponent]
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        class PlayerSpawnerAuthoringBaker : Baker<PlayerSpawnerAuthoring>
        {
            public override void Bake(PlayerSpawnerAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                var item = GetEntity(authoring.prefab,TransformUsageFlags.Dynamic);
                AddComponent(self,new PlayerSpawner(){prefab = item});
            }
        }
    }
    
    
}