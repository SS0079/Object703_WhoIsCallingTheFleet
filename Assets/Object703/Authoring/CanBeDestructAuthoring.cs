using KittyDOTS;
using Object703.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanBeDestructAuthoring : MonoBehaviour
    {
        public GameObject[] destructSpawns;
        public GameObject[] destructEffects;
        class CanBeDestructAuthoringBaker : Baker<CanBeDestructAuthoring>
        {
            public override void Bake(CanBeDestructAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                this.AddDisabledComponent(self, new DestructTag());
                if (authoring.TryGetComponent(out GhostAuthoringComponent _))
                {
                    this.AddDisabledComponent(self, new HideInClientTag());
                }
                var havedestructSpawns = authoring.destructSpawns.Length > 0;
                if (havedestructSpawns)
                {
                    var spawnPrefabs = new DestructSpawnPrefabs();
                    var count = math.min(authoring.destructSpawns.Length, 4);
                    for (int i = 0; i < count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                spawnPrefabs.value0 = GetEntity(authoring.destructSpawns[i], TransformUsageFlags.Dynamic);
                                break;
                            case 1:
                                spawnPrefabs.value1 = GetEntity(authoring.destructSpawns[i], TransformUsageFlags.Dynamic);
                                break;
                            case 2:
                                spawnPrefabs.value2 = GetEntity(authoring.destructSpawns[i], TransformUsageFlags.Dynamic);
                                break;
                            case 3:
                                spawnPrefabs.value3 = GetEntity(authoring.destructSpawns[i], TransformUsageFlags.Dynamic);
                                break;
                        }
                    }
                    AddComponent(self,spawnPrefabs);
                }
                var haveDestructEffects = authoring.destructEffects.Length>0;
                if (haveDestructEffects)
                {
                    var spawnPrefabs = new DestructEffectPrefabs();
                    var count = math.min(authoring.destructEffects.Length, 4);
                    for (int i = 0; i < count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                spawnPrefabs.value0 = GetEntity(authoring.destructEffects[i], TransformUsageFlags.Dynamic);
                                break;
                            case 1:
                                spawnPrefabs.value1 = GetEntity(authoring.destructEffects[i], TransformUsageFlags.Dynamic);
                                break;
                            case 2:
                                spawnPrefabs.value2 = GetEntity(authoring.destructEffects[i], TransformUsageFlags.Dynamic);
                                break;
                            case 3:
                                spawnPrefabs.value3 = GetEntity(authoring.destructEffects[i], TransformUsageFlags.Dynamic);
                                break;
                        }
                    }
                    AddComponent(self,spawnPrefabs);
                }
                // if (havedestructSpawns || haveDestructEffects)
                // {
                //     AddBuffer<CanDestructGhost>(self);
                // }
                
            }
        }
    }
}