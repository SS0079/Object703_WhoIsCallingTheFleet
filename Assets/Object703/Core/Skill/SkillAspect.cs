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

        public bool IsReady(NetworkTick now)
        {
            invokeAtTick.GetDataAtTick(now, out var curAtTick);
            return curAtTick.coolDownAtTick != NetworkTick.Invalid && now.IsNewerThan(curAtTick.coolDownAtTick);
        }

        public bool IsPressed(PlayerInput input)
        {
            switch (flags.ValueRO.slot)
            {
                case SkillSlot.Skill0:
                    return input.skill0.IsSet;
                case SkillSlot.Skill1:
                    return input.skill1.IsSet;
                case SkillSlot.Skill2:
                    return input.skill2.IsSet;
                case SkillSlot.Skill3:
                    return input.skill3.IsSet;
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
            invokeAtTick.AddCommandData(newTickCommand);
        }
        
    }
}