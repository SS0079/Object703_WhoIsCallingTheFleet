using Unity.Burst;
using Unity.Entities;
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
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<NetworkTime>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            localTransLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            if (!networkTime.IsFirstTimeFullyPredictingTick) return;

            // perform shot skill, also check if aim target is in skill range. if not, dont fire skill, and set skill flag to false, avoid later skill target tick reset
            foreach (var (shot, skill, owner) in
                     SystemAPI.Query<RefRO<ShotSkill>,
                             SkillAspect,
                             RefRW<GhostOwner>>()
                         .WithAll<Simulate>())
            {
                if (!skill.IsReady(networkTime)) continue;
                if (!skill.IsPressed()) continue;
            
                // stop fire skill if out of range
                if (skill.IsInRange(skill.TargetDistanceSq)) continue;
                var localShot = state.EntityManager.Instantiate(shot.ValueRO.charge);
            
                //force shot towards target position
                var rot = quaternion.LookRotationSafe(skill.AimPos - skill.OwnerPos, math.up());
                var localTrans = LocalTransform.FromPositionRotation(skill.OwnerPos, rot);
                state.EntityManager.SetComponentData(localShot, owner.ValueRO);
                state.EntityManager.SetComponentData(localShot, localTrans);
            
                if(state.WorldUnmanaged.IsServer()) continue;
                skill.StartCoolDown(networkTime.ServerTick);
            }
        }
    }
}