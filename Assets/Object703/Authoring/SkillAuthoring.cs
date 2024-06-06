using System;
using Object703.Core.Control;
using Object703.Core.Skill;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Object703.Authoring
{
   

    [DisallowMultipleComponent]
    public class SkillAuthoring : MonoBehaviour
    {
        public enum SkillType
        {
            Shot,
            Teleport,
        }
        public SkillSlot slot;
        public SkillType type;
        public SkillCommonData.AuthoringBox data;
        public GameObject spawnPrefab;
        class SkillAuthoringBaker : Baker<SkillAuthoring>
        {
            public override void Bake(SkillAuthoring authoring)
            {
                if (NetCodeConfig.Global == null) return;
                var tickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
                var self = GetEntity(TransformUsageFlags.None);
                switch (authoring.type)
                {
                    case SkillType.Shot:
                        var spawn = GetEntity(authoring.spawnPrefab,TransformUsageFlags.Dynamic);
                        AddComponent(self,new ShotSkill(){charge = spawn});
                        break;
                    case SkillType.Teleport:
                        AddComponent(self,new TeleportSkill());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                AddComponent(self,authoring.data.ToComponentData(tickRate));
                var startTick = new NetworkTick(0);
                AddComponent(self,new SkillInvokeAtTick
                {
                    coolDownAtTick = startTick,
                    lifeSpanAtTick = startTick
                });
                SetComponentEnabled<SkillInvokeAtTick>(self,true);
                AddComponent(self,new SkillFlags(){slot = authoring.slot});
            }
        }
    }
}