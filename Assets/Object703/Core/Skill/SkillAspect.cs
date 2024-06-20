using System;
using Object703.Core.Control;
using Object703.Core.NetCode;
using Unity.Entities;
using Unity.NetCode;

namespace Object703.Core.Skill
{
    public readonly partial struct SkillAspect : IAspect
    {
        private readonly RefRO<SkillCommonData> commonData;
        private readonly RefRW<SkillFlags> flags;
        private readonly DynamicBuffer<SkillInvokeAtTick> invokeAtTick;
    
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

        public bool IsPressed(PlayerSkillInput skillInput)
        {
            switch (flags.ValueRO.slot)
            {
                case SkillSlot.Skill0:
                    return skillInput.skill0.IsSet;
                case SkillSlot.Skill1:
                    return skillInput.skill1.IsSet;
                case SkillSlot.Skill2:
                    return skillInput.skill2.IsSet;
                case SkillSlot.Skill3:
                    return skillInput.skill3.IsSet;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public float Range => commonData.ValueRO.Range;
        public float RangeSq => commonData.ValueRO.RangeSq;
        public void StartCoolDown(NetworkTick now)
        {
            invokeAtTick.GetDataAtTick(now, out var newTickCommand);
            newTickCommand.coolDownAtTick = now.AddSpan(commonData.ValueRO.coolDownTick);
            newTickCommand.Tick = now.AddSpan(1u);
            invokeAtTick.AddCommandData(newTickCommand);
        }
        
    }
}