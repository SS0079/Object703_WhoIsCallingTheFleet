using Object703.Core.Control;
using Object703.Core.NetCode;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core.Skill
{
    public struct ShotSkill : IComponentData
    {
        public Entity charge;
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct ShotSkillSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransLp;
        private ComponentLookup<PlayerInput> inputLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<NetworkTime>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(false);
            inputLp = SystemAPI.GetComponentLookup<PlayerInput>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            localTransLp.Update(ref state);
            inputLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            if (!networkTime.IsFirstTimeFullyPredictingTick) return;

            // perform shot skill, also check if aim target is in skill range. if not, dont fire skill, and set skill flag to false, avoid later skill target tick reset
            foreach (var (shot, skill, owner,  ltw, parent, entity) in
                     SystemAPI.Query<RefRO<ShotSkill>,
                             SkillAspect,
                             RefRW<GhostOwner>,
                             RefRO<LocalToWorld>,
                             RefRO<Parent>>()
                         .WithAll<Simulate>().WithEntityAccess())
            {
                if (!skill.IsReady(networkTime.ServerTick)) continue;
                var performer = parent.ValueRO.Value;
                if (!inputLp.HasComponent(performer)) continue;
                var playerInput = inputLp[performer];
                if (!skill.IsPressed(playerInput)) continue;

                // stop fire skill if out of range
                var performerPos = playerInput.playerPosition;
                var distancesq = playerInput.GetSqDstFromPlayerToMouseEntity2D(localTransLp);
                if (skill.IsInRange(distancesq))
                {
                    continue;
                }
                var localShot = ecb.Instantiate(shot.ValueRO.charge);

                //force shot towards target position
                var rot = quaternion.LookRotationSafe(playerInput.mouseWorldPoint - playerInput.playerPosition, math.up());
                var localTrans = LocalTransform.FromPositionRotation(performerPos, rot);
                ecb.SetComponent(localShot, owner.ValueRO);
                ecb.SetComponent(localShot, localTrans);

                skill.StartCoolDown(networkTime.ServerTick);
            }
        }
    }
}