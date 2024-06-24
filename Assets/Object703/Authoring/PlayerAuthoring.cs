using Object703.Core.Control;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    public struct PlayerTag : IComponentData
    {
        
    }
    
    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
    }
    public class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.None);
            AddComponent(self,new PlayerTag());
            AddComponent(self,new PlayerMoveInput());
            // AddComponent(self,new PlayerSkillInput());
        }
    }
}