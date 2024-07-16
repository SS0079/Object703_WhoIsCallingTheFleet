using Object703.Core.Recycle;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core.Skill
{
    
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct TeleportSkillSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransLp;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            localTransLp = SystemAPI.GetComponentLookup<LocalTransform>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            localTransLp.Update(ref state);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            //perform teleport skill,also check if mouse aim is in skill range. perform skill at maximum range if mouse aim is out of range
            foreach (var skill in 
                     SystemAPI.Query<SkillAspect>()
                         .WithAll<Simulate,TeleportSkill>().WithNone<DestructTag>())
            {
                if(!skill.IsReady(networkTime)) continue;
                if(!skill.IsPressed()) continue;
                
                //clamp target position according to skill range
                var newPos = skill.AimPos;
                var skillOwnerPos = skill.OwnerPos;
                if (!skill.IsInRange(skill.AimDistanceSq))
                {
                    var targetDir = math.normalizesafe(newPos-skillOwnerPos);
                    newPos = skillOwnerPos + targetDir * skill.Range;
                }
                newPos = new float3(newPos.x, skillOwnerPos.y, newPos.z);
                var newRot = quaternion.LookRotationSafe(newPos - skillOwnerPos, math.up());
                var newTrans = LocalTransform.FromPositionRotation(newPos,newRot);
                localTransLp[skill.OwnerEntity] = newTrans;
            
                if(state.WorldUnmanaged.IsServer()) continue;
                skill.StartCoolDown(networkTime.ServerTick);
            }
        }


        

    }
}