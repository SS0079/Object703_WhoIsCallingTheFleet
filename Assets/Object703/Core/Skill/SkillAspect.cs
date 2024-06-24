using System;
using Object703.Core.Control;
using Object703.Core.NetCode;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core.Skill
{
    public readonly partial struct SkillAspect : IAspect
    {
        private readonly RefRO<SkillCommonData> commonData;
        private readonly RefRW<SkillFlags> flags;
        private readonly DynamicBuffer<SkillInvokeAtTick> invokeAtTick;
        private readonly RefRO<PlayerSkillInput> input;
        private readonly RefRO<Parent> owner;
    
        public bool IsInRange(float distanceSq)
        {
            return distanceSq <= commonData.ValueRO.RangeSq;
        }

        public bool IsReady(NetworkTime now)
        {
            for (uint i = 0u; i < now.SimulationStepBatchSize; i++)
            {
                var testTick = now.ServerTick;
                testTick.Subtract(i);
                if (!invokeAtTick.GetDataAtTick(testTick, out var curAtTick))
                {
                    curAtTick.coolDownAtTick=NetworkTick.Invalid;
                }
                var a = curAtTick.coolDownAtTick == NetworkTick.Invalid;
                if (a) return true;
                var b = testTick.IsNewerThan(curAtTick.coolDownAtTick);
                if (b) return true;
            }
            return false;
        }

        public bool IsPressed()
        {
            switch (flags.ValueRO.slot)
            {
                case SkillSlot.Skill0:
                    return input.ValueRO.skill0.IsSet;
                case SkillSlot.Skill1:
                    return input.ValueRO.skill1.IsSet;
                case SkillSlot.Skill2:
                    return input.ValueRO.skill2.IsSet;
                case SkillSlot.Skill3:
                    return input.ValueRO.skill3.IsSet;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float TargetDistanceSq => input.ValueRO.GetSqDstFromPlayerToMouseEntity2D();
        public float AimDistanceSq => input.ValueRO.GetSqDstFromPlayerToMousePoint2D();
        public float Range => commonData.ValueRO.Range;
        public float RangeSq => commonData.ValueRO.RangeSq;
        public float3 AimPos => input.ValueRO.mouseWorldPoint;
        public float3 OwnerPos => input.ValueRO.playerPosition;
        public Entity OwnerEntity => owner.ValueRO.Value;
        public void StartCoolDown(NetworkTick now)
        {
            invokeAtTick.GetDataAtTick(now, out var newTickCommand);
            newTickCommand.coolDownAtTick = now.AddSpan(commonData.ValueRO.coolDownTick);
            newTickCommand.Tick = now.AddSpan(1u);
            invokeAtTick.AddCommandData(newTickCommand);
        }
    }
}