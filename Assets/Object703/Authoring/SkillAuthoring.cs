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
        public NetCodeConfig netConfig;
        public SkillSlot slot;
        public SkillType type;
        public SkillCommonData.AuthoringBox data;
        public GameObject spawnPrefab;
        class SkillAuthoringBaker : Baker<SkillAuthoring>
        {
            public override void Bake(SkillAuthoring authoring)
            {
                if (authoring.netConfig == null)
                {
                    Debug.LogWarning($"Net code config missing",authoring.gameObject);
                    return;
                };
                var tickRate = authoring.netConfig.ClientServerTickRate.SimulationTickRate;
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
                AddBuffer<SkillInvokeAtTick>(self);
                AddComponent(self,new SkillFlags(){slot = authoring.slot});
                AddComponent(self,new PlayerSkillInput());
            }
        }
    }
}