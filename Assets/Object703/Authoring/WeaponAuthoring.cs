using System;
using KittyHelpYouOut;
using KittyHelpYouOut.Utilities;
using Object703.Core;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class WeaponAuthoring : MonoBehaviour
    {
        // [FormerlySerializedAs("IsAntiAir")]
        // public bool antiAir;
        // [FormerlySerializedAs("IsAntiSurface")]
        // public bool antiSurface;
        // public bool hitEnemy, hitPlayer, hitAlly, hitNeutral;
        [FormerlySerializedAs("hitFilter")]
        public HitFilterGenerator targetFilter;
        
        [FormerlySerializedAs("Weapon")]
        public Weapon.AuthoringBox weapon;
        [SerializeField]
        private bool showGizmos=false;
        

        private void OnDrawGizmos()
        {
            if (!showGizmos)return;
            Gizmos.color=Color.red;
            int t = 12;
            var midPoints = new Vector3[t+3];
            var muzzle = this.transform;
            var fanPoints = KittyMath.GetFanPoints(muzzle.position,muzzle.forward,weapon.maxRange,weapon.minRange,weapon.arcLimit,12,6);
            Gizmos.DrawLineStrip(fanPoints,true);
        }

        [ReadOnlyInspector]
        public ColliderLayers belongTo;
        [ReadOnlyInspector]
        public ColliderLayers collideWith;
        
        [ContextMenu("DebugFilter")]
        public void DebugFilter()
        {
            var filter = targetFilter.GetFilter();
            
            this.collideWith = (ColliderLayers)filter.CollidesWith;
            this.belongTo = (ColliderLayers)filter.BelongsTo;
        }

    }

    [Serializable]
    public class HitFilterGenerator
    {
        public bool antiAir,antiSurface,hitTerran;
        public CanBeHitAuthoring.Role targetRole;

        public CollisionFilter GetFilter()
        {
            uint collideWith = hitTerran? (uint)ColliderLayers.Terran : 0;
            uint belongTo = ColliderLayers.Caster.ToUInt();
            switch (targetRole)
            {
                case CanBeHitAuthoring.Role.Player:
                    collideWith |= antiAir ? ColliderLayers.AirbornePlayer.ToUInt() : 0;
                    collideWith |= antiSurface ? ColliderLayers.SurfacePlayer.ToUInt() : 0;
                    break;
                case CanBeHitAuthoring.Role.Ally:
                    collideWith |= antiAir ? ColliderLayers.AirborneAlly.ToUInt() : 0;
                    collideWith |= antiSurface ? ColliderLayers.SurfaceAlly.ToUInt() : 0;
                    break;
                case CanBeHitAuthoring.Role.Enemy:
                    collideWith |= antiAir ? ColliderLayers.AirborneEnemy.ToUInt() : 0;
                    collideWith |= antiSurface ? ColliderLayers.SurfaceEnemy.ToUInt() : 0;
                    break;
                case CanBeHitAuthoring.Role.Neutral:
                    collideWith |= ColliderLayers.Neutral.ToUInt();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new CollisionFilter
            {
                BelongsTo = belongTo,
                CollidesWith = collideWith,
                GroupIndex = 0
            };
        }
    } 
        
        
    public class WeaponAuthoringBaker : Baker<WeaponAuthoring>
    {
        public override void Bake(WeaponAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            var weapon = authoring.weapon.ToComponentData(this);
            var filter = authoring.targetFilter.GetFilter();
            weapon.targetFilter = filter;
            AddComponent(self,weapon);
            var randomData = new IndividualRandom() { value = Random.CreateFromIndex((uint)self.Index) };
            AddComponent(self,randomData);
            AddBuffer<TargetBuffer>(self);
            if (authoring.TryGetComponent(out GhostAuthoringComponent _))
            {
                AddBuffer<ShootAtTick>(self);
                // AppendToBuffer(self,new ShootAtTick(){Tick = new NetworkTick(1),coolDownAtTick = new NetworkTick(100)});
            }
        }
    }
}