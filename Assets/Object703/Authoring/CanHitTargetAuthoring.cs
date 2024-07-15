using System;
using KittyHelpYouOut.Utilities;
using Object703.Core.Combat;
using Object703.Core.Recycle;
using Object703.Core.VisualEffect;
using Object703.Core.Weapon;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Authoring
{
    public enum HitType
    {
        SphereCast,
        SphereOverlap,
        Distance
    }
    
    [DisallowMultipleComponent]
    public class CanHitTargetAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("Hit")]
        public HitType hit;
        [FormerlySerializedAs("Radius")]
        public float radius;
        public HitFilterGenerator hitFilter;
        [FormerlySerializedAs("Damage")]
        public float damage;
        [FormerlySerializedAs("HitSpawns")]
        public GameObject[] hitSpawns;
        public GameObject[] hitEffects;
        
        [ReadOnlyInspector]
        public ColliderLayers belongTo;
        [ReadOnlyInspector]
        public ColliderLayers collideWith;
        [ContextMenu("DebugFilter")]
        public void DebugFilter()
        {
            var filter = hitFilter.GetFilter();
            
            this.collideWith = (ColliderLayers)filter.CollidesWith;
            this.belongTo = (ColliderLayers)filter.BelongsTo;
        }
        
    }
    public class CanTargetAuthoringBaker : Baker<CanHitTargetAuthoring>
    {
        public override void Bake(CanHitTargetAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            var filter = authoring.hitFilter.GetFilter();
            switch (authoring.hit)
            {
                case HitType.SphereCast:
                    AddComponent(self,new SphereCastHitCheck
                    {
                        radius = authoring.radius,
                        lastPos = default,
                        filter = filter
                    });
                    break;
                case HitType.SphereOverlap:
                    AddComponent(self,new SphereOverlapCheck
                    {
                        radius = authoring.radius,
                        filter = filter
                    });
                    break;
                case HitType.Distance:
                    AddComponent(self,new HomingCheckDistance(){Value = authoring.radius});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (authoring.damage>0)
            {
                AddComponent(self,new DealDamage(){value = authoring.damage});
            }
            AddBuffer<HitCheckResult>(self);
            if (authoring.hitSpawns.Length > 0)
            {
                var spawnPrefabs = new HitSpawnPrefabs();
                var count = math.min(authoring.hitSpawns.Length, 4);
                for (int i = 0; i < count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            spawnPrefabs.value0 = GetEntity(authoring.hitSpawns[i], TransformUsageFlags.Dynamic);
                            break;
                        case 1:
                            spawnPrefabs.value1 = GetEntity(authoring.hitSpawns[i], TransformUsageFlags.Dynamic);
                            break;
                        case 2:
                            spawnPrefabs.value2 = GetEntity(authoring.hitSpawns[i], TransformUsageFlags.Dynamic);
                            break;
                        case 3:
                            spawnPrefabs.value3 = GetEntity(authoring.hitSpawns[i], TransformUsageFlags.Dynamic);
                            break;
                    }
                }
                AddComponent(self,spawnPrefabs);
            }
            if (authoring.hitEffects.Length>0)
            {
                var spawnPrefabs = new HitEffectPrefabs();
                var count = math.min(authoring.hitEffects.Length, 4);
                for (int i = 0; i < count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            spawnPrefabs.value0 = GetEntity(authoring.hitEffects[i], TransformUsageFlags.Dynamic);
                            break;
                        case 1:
                            spawnPrefabs.value1 = GetEntity(authoring.hitEffects[i], TransformUsageFlags.Dynamic);
                            break;
                        case 2:
                            spawnPrefabs.value2 = GetEntity(authoring.hitEffects[i], TransformUsageFlags.Dynamic);
                            break;
                        case 3:
                            spawnPrefabs.value3 = GetEntity(authoring.hitEffects[i], TransformUsageFlags.Dynamic);
                            break;
                    }
                }
                AddComponent(self,spawnPrefabs);
            }
        }
    }
}