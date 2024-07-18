using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    public struct PlayerAssetTag : IComponentData
    {
        
    }
    
    [DisallowMultipleComponent]
    public class PlayerAssetAuthoring : MonoBehaviour
    {
        
    }
    public class PlayerAssetAuthoringBaker : Baker<PlayerAssetAuthoring>
    {
        public override void Bake(PlayerAssetAuthoring authoring)
        {
            var self = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(self,new PlayerAssetTag());
        }
    }
}