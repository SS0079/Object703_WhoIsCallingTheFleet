using System;
using Object703.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Object703.Core
{
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerMoveInput : IInputComponentData
    {
        [GhostField]public float forwardBackward;
        [GhostField]public float leftRight;
        [GhostField]public float turn;
        [GhostField]public float2 mouseDelta;
        [GhostField]public float mouseScroll;
        
        
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerSkillInput : IInputComponentData
    {
        [GhostField]public float3 playerPosition,mouseWorldPoint,aimEntityPosition;
        public readonly float GetSqDstFromPlayerToMousePoint2D() => math.distancesq(new float3(playerPosition.x, 0, playerPosition.z), new float3(mouseWorldPoint.x, 0, mouseWorldPoint.z));
        public readonly float GetSqDstFromPlayerToMouseEntity2D()
        {
            if (mousePointEntity == Entity.Null) return -1;
            return math.distancesq(new float3(playerPosition.x, 0, playerPosition.z), new float3(aimEntityPosition.x, 0, aimEntityPosition.z));
        }
        [GhostField]public Entity mousePointEntity;
        [GhostField]public InputEvent skill0, skill1, skill2, skill3;

        public readonly bool CheckPress(SkillSlot slot)
        {
            switch (slot)
            {
                case SkillSlot.Skill0:
                    return skill0.IsSet;
                case SkillSlot.Skill1:
                    return skill1.IsSet;
                case SkillSlot.Skill2:
                    return skill2.IsSet;
                case SkillSlot.Skill3:
                    return skill3.IsSet;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }
    }
    
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct PlayerInputSystem : ISystem
    {
        private CollisionFilter mouseClickFilter;
        private ComponentLookup<LocalToWorld> ltwLp;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerMoveInput>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EnableGameTag>();
            mouseClickFilter = new CollisionFilter
            {
                BelongsTo = (uint)ColliderLayers.Caster,
                CollidesWith = (uint)ColliderLayers.Terran | (uint)ColliderLayers.SurfaceEnemy | (uint)ColliderLayers.AirborneEnemy,
                GroupIndex = 0
            };
            ltwLp = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            // handle player move input
            var input = new PlayerMoveInput();
            //gather keyboard WSAD control input
            input.forwardBackward = PlayerInputManager.Instance.forwardBackward;
            input.leftRight = PlayerInputManager.Instance.leftRight;
            input.turn = PlayerInputManager.Instance.turn;
            //gather mouse input and screen point hit result
            input.mouseDelta = PlayerInputManager.Instance.mouseDelta;
            input.mouseScroll = PlayerInputManager.Instance.mouseScroll;
            foreach (var (moveInput,localTrans) in SystemAPI.Query<RefRW<PlayerMoveInput>,RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
            {
                //write player current position by the way
                moveInput.ValueRW = input;
            }

            //================================================================================
            // handle player skill input
            var cam = Camera.main;
            if (cam == null) return;
            ltwLp.Update(ref state);
            var skill = new PlayerSkillInput();
            var cWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var curMousePos = Input.mousePosition;
            var mouseRay = cam.ScreenPointToRay(curMousePos);
            RaycastInput ray = new RaycastInput
            {
                Filter = mouseClickFilter,
                Start = mouseRay.origin,
                End = mouseRay.GetPoint(1000)
            };
            cWorld.CastRay(ray, out RaycastHit hit);
            skill.mouseWorldPoint = hit.Position;
            skill.mousePointEntity = hit.Entity;
            if (ltwLp.HasComponent(skill.mousePointEntity))
            {
                skill.aimEntityPosition = ltwLp[skill.mousePointEntity].Position;
            }
            //gather key bit
            if (PlayerInputManager.Instance.skill0)
            {
                skill.skill0.Set();
            }
            if (PlayerInputManager.Instance.skill1)
            {
                skill.skill1.Set();
            }
            if (PlayerInputManager.Instance.skill2)
            {
                skill.skill2.Set();
            }
            if (PlayerInputManager.Instance.skill3)
            {
                skill.skill3.Set();
            }
            foreach (var (skillInput,parent) in SystemAPI.Query<RefRW<PlayerSkillInput>, RefRO<Parent>>().WithAll<GhostOwnerIsLocal>())
            {
                //write player current position by the way
                skillInput.ValueRW = skill;
                skillInput.ValueRW.playerPosition = ltwLp[parent.ValueRO.Value].Position;
            }
        }
    }
}