using System;
using Object703.Core;
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
        
        // [ReadOnlyInspector]
        // public ColliderLayers belongTo;
        // [ReadOnlyInspector]
        // public ColliderLayers collideWith;
        // [ContextMenu("DebugFilter")]
        // public void DebugFilter()
        // {
        //     var filter = hitFilter.GetFilter();
        //     
        //     this.collideWith = (ColliderLayers)filter.CollidesWith;
        //     this.belongTo = (ColliderLayers)filter.BelongsTo;
        // }
        //
        // public 
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
            var collideWith = ColliderLayers.Caster.ToUInt();
            authoring.GetComponent<PhysicsShapeAuthoring>().BelongsTo = new PhysicsCategoryTags() { Value = belongTo };
            authoring.GetComponent<PhysicsShapeAuthoring>().CollidesWith = new PhysicsCategoryTags() { Value = collideWith };
            AddComponent(self,new Hp(){max = authoring.maxHp,current = authoring.maxHp});
            AddBuffer<DamageBuffer>(self);
            AddBuffer<DamageThisTick>(self);
        }
    }
}