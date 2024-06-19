﻿using System;
using Object703.Authoring;
using Object703.Core.NetCode;
using Object703.Core.Weapon;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Object703.Core.Control
{
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerInput : IInputComponentData
    {
        [GhostField]public float forwardBackward;
        [GhostField]public float leftRight;
        [GhostField]public float turn;
        [GhostField]public float2 mouseDelta;
        [GhostField]public float mouseScroll;
        [GhostField]public float3 playerPosition,mouseWorldPoint;
        public float GetSqDstFromPlayerToMousePoint2D() => math.distancesq(new float3(playerPosition.x, 0, playerPosition.z), new float3(mouseWorldPoint.x, 0, mouseWorldPoint.z));
        public float GetSqDstFromPlayerToMouseEntity2D(ComponentLookup<LocalTransform> transLp)
        {
            if (!transLp.HasComponent(mousePointEntity)) return -1f;
            var targetPos = transLp[mousePointEntity].Position;
            return math.distancesq(new float3(playerPosition.x, 0, playerPosition.z), new float3(targetPos.x, 0, targetPos.z));
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
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EnableGameTag>();
            state.RequireForUpdate<PlayerInput>();
            mouseClickFilter = new CollisionFilter
            {
                BelongsTo = (uint)ColliderLayers.Caster,
                CollidesWith = (uint)ColliderLayers.Terran | (uint)ColliderLayers.SurfaceEnemy | (uint)ColliderLayers.AirborneEnemy,
                GroupIndex = 0
            };
        }

        public void OnUpdate(ref SystemState state)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            var input = new PlayerInput();
            // var bit = new PlayerBit();

            //gather keyboard WSAD control input
            input.forwardBackward = PlayerInputManager.Instance.forwardBackward;
            input.leftRight = PlayerInputManager.Instance.leftRight;
            input.turn = PlayerInputManager.Instance.turn;
            //gather mouse input and screen point hit result
            input.mouseDelta = PlayerInputManager.Instance.mouseDelta;
            input.mouseScroll = PlayerInputManager.Instance.mouseScroll;
            var cWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            // var curMousePos = Mouse.current.position;
            var curMousePos = Input.mousePosition;
            var mouseRay = cam.ScreenPointToRay(curMousePos);
            RaycastInput ray = new RaycastInput
            {
                Filter = mouseClickFilter,
                Start = mouseRay.origin,
                End = mouseRay.GetPoint(1000)
            };
            cWorld.CastRay(ray, out RaycastHit hit);
            input.mouseWorldPoint = hit.Position;
            input.mousePointEntity = hit.Entity;
            
            //gather key bit
            if (PlayerInputManager.Instance.skill0)
            {
                input.skill0.Set();
            }
            if (PlayerInputManager.Instance.skill1)
            {
                input.skill1.Set();
            }
            if (PlayerInputManager.Instance.skill2)
            {
                input.skill2.Set();
            }
            if (PlayerInputManager.Instance.skill3)
            {
                input.skill3.Set();
            }
            foreach (var (playerInput,localTrans) in SystemAPI.Query<RefRW<PlayerInput>,RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
            {
                //write player current position by the way
                input.playerPosition = localTrans.ValueRO.Position;
                playerInput.ValueRW = input;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

    }
}