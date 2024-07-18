using Object703.Core.OnHit;
using Unity.Entities;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class IsObstacleAuthoring : MonoBehaviour
    {
        
        class IsObstacleAuthoringBaker : Baker<IsObstacleAuthoring>
        {
            public override void Bake(IsObstacleAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                AddComponent(self,new ObstacleTag());
            }
        }
    }
}