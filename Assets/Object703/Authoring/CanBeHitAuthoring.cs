using System;
using Object703.Core.OnHit;
using Object703.Core.Weapon;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanBeHitAuthoring : MonoBehaviour
    {
        public enum Role
        {
            Player,
            Ally,
            Enemy,
            Neutral
        }
        public bool isAirBorne,isSurface;
        public Role role;
        
        [FormerlySerializedAs("MaxHp")]
        public float maxHp;
    }
    public class CanBeHitAuthoringBaker : Baker<CanBeHitAuthoring>
    {
        public override void Bake(CanBeHitAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            uint belongTo = 0;
            switch (authoring.role)
            {
                case CanBeHitAuthoring.Role.Player:
                    if (authoring.isSurface)
                    {
                        belongTo |= (uint)ColliderLayers.SurfacePlayer;
                    }
                    if (authoring.isAirBorne)
                    {
                        belongTo |= (uint)ColliderLayers.AirbornePlayer;
                    }
                    break;
                case CanBeHitAuthoring.Role.Ally:
                    if (authoring.isSurface)
                    {
                        belongTo |= (uint)ColliderLayers.SurfaceAlly;
                    }
                    if (authoring.isAirBorne)
                    {
                        belongTo |= (uint)ColliderLayers.AirborneAlly;
                    }
                    break;
                case CanBeHitAuthoring.Role.Enemy:
                    if (authoring.isSurface)
                    {
                        belongTo |= (uint)ColliderLayers.SurfaceEnemy;
                    }
                    if (authoring.isAirBorne)
                    {
                        belongTo |= (uint)ColliderLayers.AirborneEnemy;
                    }
                    break;
                case CanBeHitAuthoring.Role.Neutral:
                    belongTo |= (uint)ColliderLayers.Neutral;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            authoring.GetComponent<PhysicsShapeAuthoring>().BelongsTo = new PhysicsCategoryTags() { Value = belongTo };
            AddComponent(self,new Hp(){max = authoring.maxHp,current = authoring.maxHp});
            AddBuffer<DamageBuffer>(self);
            AddBuffer<DamageThisTick>(self);
        }
    }
}