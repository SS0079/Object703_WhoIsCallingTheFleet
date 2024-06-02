﻿using Object703.Authoring;
using Object703.Core.Moving;
using Object703.Core.Skill;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core.Control
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PlayerControlSystem : ISystem
    {
        private ComponentLookup<PlayerInput> playerInputLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<PlayerInput>();
            playerInputLp=SystemAPI.GetComponentLookup<PlayerInput>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            playerInputLp.Update(ref state);
            new PlayerControlJobs.PlayerMoveControlJob().Run();
            new PlayerControlJobs.CheckSkillTriggerJob
            {
                inputLp = playerInputLp
            }.Run();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        
    }
    

    public partial struct PlayerControlJobs
    {
        /// <summary>
        /// modify the move and rotate of player according to gathered keyboard WSAD input
        /// </summary>
        [BurstCompile]
        public partial struct PlayerMoveControlJob : IJobEntity
        {
            public void Execute(
                ref MoveAxis moveAxis,
                in PlayerInput input,
                ref RotateAxis rotateAxis)
            {
                moveAxis.moveDirection = new float3(input.leftRight, 0, input.forwardBackward);
                rotateAxis.rotateEuler = new float3(0, input.turn, 0);
            }
        }

        /// <summary>
        /// modify the skill flags of player assets according to gathered alphabet number key input
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(PlayerAssetTag),typeof(GhostOwnerIsLocal))]
        public partial struct CheckSkillTriggerJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<PlayerInput> inputLp;
            public void Execute(
                in Parent parent,
                ref SkillFlags flags)
            {
                var intput = inputLp[parent.Value];
                flags.skillTriggerDown = intput.CheckPress(flags.slot);
            }
        }

        // /// <summary>
        // /// update skill target according to gathered mouse input
        // /// </summary>
        // [BurstCompile]
        // [WithAll(typeof(PlayerAssetTag))]
        // public partial struct UpdateSkillCommonDataJob : IJobEntity
        // {
        //     // public NativeHashMap<int, PlayerInput> inputMap;
        //     [ReadOnly]
        //     public ComponentLookup<PlayerInput> playerInputLp;
        //     public void Execute(
        //         in GhostOwner owner,
        //         ref SkillCommonData commonData,
        //         in LocalToWorld ltw,
        //         in Parent parent)
        //     {
        //         if (playerInputLp.TryGetComponent(parent.Value,out var input))
        //         {
        //             commonData.target = input.mousePointEntity;
        //             commonData.from = ltw.Position;
        //             commonData.to = input.mouseWorldPoint;
        //             commonData.owner = parent.Value;
        //         }
        //     }
        // }

    }

}