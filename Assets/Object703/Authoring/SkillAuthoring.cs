using System;
using KittyHelpYouOut.Utilities;
using Object703.Core.Control;
using Object703.Core.Skill;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703.Authoring
{
   

    [DisallowMultipleComponent]
    public class SkillAuthoring : MonoBehaviour
    {
        [Serializable]
        public struct Skill
        {
            public SkillSlot slot;
            public SkillType type;
            public SkillCommonData data;
            public GameObject spawnPrefab;
        }
        public enum SkillType
        {
            Shot,
            Teleport,
        }
        public Skill skill;
        class SkillAuthoringBaker : Baker<SkillAuthoring>
        {
            public override void Bake(SkillAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                switch (authoring.skill.type)
                {
                    case SkillType.Shot:
                        var spawn = GetEntity(authoring.skill.spawnPrefab,TransformUsageFlags.Dynamic);
                        AddComponent(self,new ShotSkill(){charge = spawn});
                        break;
                    case SkillType.Teleport:
                        AddComponent(self,new TeleportSkill());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                AddComponent(self,authoring.skill.data);
                AddComponent(self,new SkillInvokeAtTick());
                SetComponentEnabled<SkillInvokeAtTick>(self,false);
                AddComponent(self,new SkillFlags(){slot = authoring.skill.slot});
            }
        }
    }
}