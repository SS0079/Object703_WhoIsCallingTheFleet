using System;
using Object703.Core.NetCode;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class EntityPrefabAuthoring : MonoBehaviour
    {
        public GameObject[] prefabs;
        public class EntityPrefabAuthoringBaker : Baker<EntityPrefabAuthoring>
        {
            public override void Bake(EntityPrefabAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                if (authoring.prefabs.Length > 0)
                {
                    AddBuffer<WeaponAndSkillPrefab>(self);
                    for (int i = 0; i < authoring.prefabs.Length; i++)
                    {
                        var name = authoring.prefabs[i].name;
                        var prefab = GetEntity(authoring.prefabs[i], TransformUsageFlags.Dynamic);
                        AppendToBuffer(self,new WeaponAndSkillPrefab(){name = new FixedString32Bytes(name),value = prefab});
                    }
                }
            }
        }
    }
    
}